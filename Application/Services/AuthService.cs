using Application.Common;
using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.Contracts.Services.Hashing;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentials = "Invalid Email or Password";
        private const string InvalidToken = "The link is invalid or has expired.";

        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepo;
        private readonly IUserTokenRepository _tokenRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IEmailSender _email;
        private readonly AuthSettings _settings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork uow,
            IMapper mapper,
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IUserTokenRepository tokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwt,
            IEmailSender email,
            AuthSettings settings,
            ILogger<AuthService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _currentUser = currentUser;
            _userRepo = userRepository;
            _tokenRepo = tokenRepository;
            _passwordHasher = passwordHasher;
            _jwt = jwt;
            _email = email;
            _settings = settings;
            _logger = logger;
        }

        // ── Registration & login ─────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new BadRequestException("Email and Password are required.");
            if (request.Password != request.ConfirmPassword)
                throw new BadRequestException("Password and Confirm Password do not match.");
            if (!Application.Validation.StrongPasswordAttribute.Check(request.Password))
                throw new BadRequestException("Password must be 8-64 characters and contain an uppercase letter, a lowercase letter and a digit.");

            var email = NormalizeEmail(request.Email);
            var phone = request.Phone.Trim();

            if (await _userRepo.EmailExistsAsync(email))
                throw new ConflictException("Email is already taken.");
            if (await _userRepo.PhoneExistsAsync(phone))
                throw new ConflictException("Phone number is already used.");

            if (!Enum.TryParse<Gender>(request.Gender, true, out var gender) || !Enum.IsDefined(gender))
                throw new BadRequestException("Invalid gender value.");

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                HashPassword = _passwordHasher.Hash(request.Password),
                Phone = phone,
                Gender = gender,
                Role = UserRole.Customer
            };

            await _uow.GetRepository<User, Guid>().AddAsync(user);
            await _uow.SaveChangesAsync();

            await SendConfirmationEmailAsync(user, cancellationToken);

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new UnauthorizedException(InvalidCredentials);

            var user = await _userRepo.GetByEmailAsync(NormalizeEmail(request.Email));
            if (user is null)
                throw new UnauthorizedException(InvalidCredentials);

            if (user.LockoutEnd is { } lockoutEnd && lockoutEnd > DateTime.UtcNow)
            {
                var minutes = (int)Math.Ceiling((lockoutEnd - DateTime.UtcNow).TotalMinutes);
                throw new UnauthorizedException($"Too many failed attempts. Account is locked, try again in {minutes} minute(s).");
            }

            if (!_passwordHasher.Verify(request.Password, user.HashPassword))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= _settings.MaxFailedLoginAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(_settings.LockoutMinutes);
                    user.FailedLoginAttempts = 0;
                    _logger.LogWarning("Account {UserId} locked for {Minutes} minutes after repeated failed logins.", user.Id, _settings.LockoutMinutes);
                }
                await _uow.SaveChangesAsync();
                throw new UnauthorizedException(InvalidCredentials);
            }

            if (user.FailedLoginAttempts != 0 || user.LockoutEnd is not null)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _uow.SaveChangesAsync();
            }

            return await IssueTokensAsync(user);
        }

        // ── Refresh tokens ───────────────────────────────────────────────────

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _tokenRepo.GetByHashAsync(UserTokenType.RefreshToken, TokenHelper.Hash(refreshToken));
            if (token is null)
                throw new UnauthorizedException("Invalid refresh token.");

            if (token.RevokedAt is not null)
            {
                // A rotated/revoked token is being replayed: assume theft and kill every session of this user.
                _logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all sessions.", token.UserId);
                await _tokenRepo.RevokeAllAsync(token.UserId, UserTokenType.RefreshToken);
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token has expired. Please log in again.");

            var user = await _uow.GetRepository<User, Guid>().GetByIdAsync(token.UserId);
            if (user is null)
                throw new UnauthorizedException("Invalid refresh token.");

            token.RevokedAt = DateTime.UtcNow;   // rotation: each refresh token is single-use
            return await IssueTokensAsync(user);
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _tokenRepo.GetByHashAsync(UserTokenType.RefreshToken, TokenHelper.Hash(refreshToken));
            if (token is not null && token.RevokedAt is null)
            {
                token.RevokedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
        }

        public Task LogoutAllAsync(CancellationToken cancellationToken = default)
            => _tokenRepo.RevokeAllAsync(_currentUser.UserId, UserTokenType.RefreshToken);

        // ── Profile ──────────────────────────────────────────────────────────

        public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserEntityAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateProfileAsync(UpdateUserDto request, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserEntityAsync();

            if (request.FirstName is not null && string.IsNullOrWhiteSpace(request.FirstName))
                throw new BadRequestException("First name cannot be empty.");
            if (request.LastName is not null && string.IsNullOrWhiteSpace(request.LastName))
                throw new BadRequestException("Last name cannot be empty.");

            if (request.Phone is not null)
            {
                var phone = request.Phone.Trim();
                if (phone.Length == 0)
                    throw new BadRequestException("Phone cannot be empty.");
                if (phone != user.Phone && await _userRepo.PhoneExistsAsync(phone, user.Id))
                    throw new ConflictException("Phone number is already used.");
                request.Phone = phone;
            }

            request.FirstName = request.FirstName?.Trim();
            request.LastName = request.LastName?.Trim();

            _mapper.Map(request, user);
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task ChangePasswordAsync(ChangePasswordDto request, CancellationToken cancellationToken = default)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                throw new BadRequestException("New password and confirmation do not match.");
            if (!Application.Validation.StrongPasswordAttribute.Check(request.NewPassword))
                throw new BadRequestException("Password must be 8-64 characters and contain an uppercase letter, a lowercase letter and a digit.");

            var user = await GetCurrentUserEntityAsync();

            if (!_passwordHasher.Verify(request.CurrentPassword, user.HashPassword))
                throw new BadRequestException("Current password is incorrect.");
            if (request.CurrentPassword == request.NewPassword)
                throw new BadRequestException("New password must be different from the current password.");

            user.HashPassword = _passwordHasher.Hash(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            // Sign out every other session
            await _tokenRepo.RevokeAllAsync(user.Id, UserTokenType.RefreshToken);

            var (subject, html) = EmailTemplates.PasswordChanged(user.FirstName);
            await TrySendAsync(user.Email, subject, html, cancellationToken);
        }

        // ── Email confirmation & password reset ──────────────────────────────

        public async Task ConfirmEmailAsync(ConfirmEmailDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(NormalizeEmail(request.Email));
            var token = await _tokenRepo.GetByHashAsync(UserTokenType.EmailConfirmation, TokenHelper.Hash(request.Token));

            if (user is null || token is null || token.UserId != user.Id || !token.IsActive)
                throw new BadRequestException(InvalidToken);

            token.RevokedAt = DateTime.UtcNow;
            user.EmailConfirmed = true;
            await _uow.SaveChangesAsync();
        }

        public async Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // Never reveal whether the email exists
            var user = await _userRepo.GetByEmailAsync(NormalizeEmail(email));
            if (user is not null && !user.EmailConfirmed)
                await SendConfirmationEmailAsync(user, cancellationToken);
        }

        public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByEmailAsync(NormalizeEmail(email));
            if (user is null) return;   // same response either way

            await _tokenRepo.RevokeAllAsync(user.Id, UserTokenType.PasswordReset);
            var raw = await CreateTokenAsync(user.Id, UserTokenType.PasswordReset, TimeSpan.FromMinutes(_settings.PasswordResetMinutes));

            var link = BuildLink("/reset-password", user.Email, raw);
            var (subject, html) = EmailTemplates.ResetPassword(user.FirstName, link, _settings.PasswordResetMinutes);
            await TrySendAsync(user.Email, subject, html, cancellationToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                throw new BadRequestException("New password and confirmation do not match.");
            if (!Application.Validation.StrongPasswordAttribute.Check(request.NewPassword))
                throw new BadRequestException("Password must be 8-64 characters and contain an uppercase letter, a lowercase letter and a digit.");

            var user = await _userRepo.GetByEmailAsync(NormalizeEmail(request.Email));
            var token = await _tokenRepo.GetByHashAsync(UserTokenType.PasswordReset, TokenHelper.Hash(request.Token));

            if (user is null || token is null || token.UserId != user.Id || !token.IsActive)
                throw new BadRequestException(InvalidToken);

            token.RevokedAt = DateTime.UtcNow;
            user.HashPassword = _passwordHasher.Hash(request.NewPassword);
            user.EmailConfirmed = true;            // receiving the reset email proves control of the address
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            await _tokenRepo.RevokeAllAsync(user.Id, UserTokenType.RefreshToken);

            var (subject, html) = EmailTemplates.PasswordChanged(user.FirstName);
            await TrySendAsync(user.Email, subject, html, cancellationToken);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private async Task<User> GetCurrentUserEntityAsync()
        {
            var user = await _uow.GetRepository<User, Guid>().GetByIdAsync(_currentUser.UserId);
            return user ?? throw new NotFoundException($"User with ID {_currentUser.UserId} was not found.");
        }

        /// <summary>Creates a hashed, expiring token row and returns the RAW token (only time it exists in clear).</summary>
        private async Task<string> CreateTokenAsync(Guid userId, UserTokenType type, TimeSpan lifetime)
        {
            var raw = TokenHelper.Generate();
            await _tokenRepo.AddAsync(new UserToken
            {
                UserId = userId,
                Type = type,
                TokenHash = TokenHelper.Hash(raw),
                ExpiresAt = DateTime.UtcNow.Add(lifetime)
            });
            await _uow.SaveChangesAsync();
            return raw;
        }

        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            await _tokenRepo.DeleteExpiredAsync(user.Id);   // housekeeping

            var (accessToken, accessExpiry) = _jwt.GenerateToken(user);

            var refreshLifetime = TimeSpan.FromDays(_settings.RefreshTokenDays);
            var refreshRaw = await CreateTokenAsync(user.Id, UserTokenType.RefreshToken, refreshLifetime);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                EmailConfirmed = user.EmailConfirmed,
                Token = accessToken,
                TokenExpiry = accessExpiry,
                RefreshToken = refreshRaw,
                RefreshTokenExpiry = DateTime.UtcNow.Add(refreshLifetime)
            };
        }

        private async Task SendConfirmationEmailAsync(User user, CancellationToken ct)
        {
            await _tokenRepo.RevokeAllAsync(user.Id, UserTokenType.EmailConfirmation);
            var raw = await CreateTokenAsync(user.Id, UserTokenType.EmailConfirmation, TimeSpan.FromHours(_settings.EmailConfirmationHours));

            var link = BuildLink("/confirm-email", user.Email, raw);
            var (subject, html) = EmailTemplates.ConfirmEmail(user.FirstName, link, _settings.EmailConfirmationHours);
            await TrySendAsync(user.Email, subject, html, ct);
        }

        private string BuildLink(string path, string email, string token)
            => $"{_settings.FrontendUrl.TrimEnd('/')}{path}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        /// <summary>Email failures must never break the main flow (registration, reset...). They are logged.</summary>
        private async Task TrySendAsync(string to, string subject, string html, CancellationToken ct)
        {
            try { await _email.SendAsync(to, subject, html, ct); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to send email '{Subject}' to {To}", subject, to); }
        }
    }
}
