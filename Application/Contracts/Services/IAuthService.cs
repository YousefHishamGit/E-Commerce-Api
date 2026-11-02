using Application.DTOs.Auth;
using Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
        Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
