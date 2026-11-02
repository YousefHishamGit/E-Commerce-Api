using Application.Contracts.Services;
using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace E_Commerce.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register(
            [FromBody] RegisterDto request,
            CancellationToken cancellationToken)
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(response, "User registered successfully."));
        }

        /// <summary>
        /// Login with email and password to receive a JWT token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login(
            [FromBody] LoginDto request,
            CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(response, "Login successful."));
        }

        /// <summary>
        /// Get current authenticated user details from JWT token.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUser(
            CancellationToken cancellationToken)
        {
            var user = await _authService.GetCurrentUserAsync(cancellationToken);
            return Ok(ApiResponseDto<UserDto>.Ok(user));
        }
    }
}

