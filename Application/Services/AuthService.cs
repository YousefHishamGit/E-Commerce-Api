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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _UoW;
        private readonly IMapper _Mapper;
        private readonly ICurrentUser _CurrentUser;
        private readonly IUserRepository _UserRepo;
        private readonly IPasswordHasher _PasswordHasher;
        private readonly IJwtTokenGenerator _JwtGenerator;

        public AuthService(IUnitOfWork uow, IMapper mapper,ICurrentUser currentUser,IUserRepository userRepository,IPasswordHasher passwordHasher,IJwtTokenGenerator jwt ) { 
            _UoW= uow;
            _Mapper= mapper;
            _CurrentUser= currentUser;
            _UserRepo= userRepository;
            _PasswordHasher= passwordHasher;
            _JwtGenerator = jwt;
        }

        public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var user = await _UoW.GetRepository<User,Guid>().GetByIdAsync(_CurrentUser.UserId);
            if (user is null)
            {
                throw new NotFoundException($"User with ID {_CurrentUser.UserId} was not found.");
            }
            return _Mapper.Map<UserDto>(user);

        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email)|| string.IsNullOrWhiteSpace(request.Password))
            {
                throw new UnauthorizedException("Invalid Email or Password ");
            }
            var user = await _UserRepo.GetByEmailAsync(request.Email);
            if (user is null)
            {
                throw new UnauthorizedException("Invalid Email or Password ");
            }
            var isPasswordValid = _PasswordHasher.Verify(request.Password, user.HashPassword);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Invalid Email or Password ");
            }
            var token = _JwtGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                Token = token
            };

        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new BadRequestException("Email and Password are required.");
            }
            if (request.Password != request.ConfirmPassword)
            {
                throw new BadRequestException("Password and Confirm Password do not match.");
            }
            var IsEmailTaken =await _UserRepo.EmailExistsAsync(request.Email);
            if (IsEmailTaken)
            {
                throw new ConflictException("Email is already taken.");
            }

            if (!Enum.TryParse<Gender>(request.Gender, true, out var gender) || !Enum.IsDefined(gender))
                throw new BadRequestException("Invalid gender value.");
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                HashPassword = _PasswordHasher.Hash(request.Password),
                Phone = request.Phone,
                Gender = gender,
                Role = UserRole.Customer
            };

            await _UoW.GetRepository<User, Guid>().AddAsync(user);
            await _UoW.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                Token = _JwtGenerator.GenerateToken(user)
            };

        }
    }
}
