using Application.DTOs.Auth;
using Application.DTOs.Users;

namespace Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAllAsync(CancellationToken cancellationToken = default);

        Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
        Task<UserDto> UpdateProfileAsync(UpdateUserDto request, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(ChangePasswordDto request, CancellationToken cancellationToken = default);

        Task ConfirmEmailAsync(ConfirmEmailDto request, CancellationToken cancellationToken = default);
        Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken = default);
        Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
    }
}
