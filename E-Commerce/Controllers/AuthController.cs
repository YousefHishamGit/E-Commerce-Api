using Application.Contracts.Services;
using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_Commerce.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new customer account. A confirmation email is sent.</summary>
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register(
            [FromBody] RegisterDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(response, "User registered successfully. Please confirm your email."));
        }

        /// <summary>Login with email and password. Locks the account after repeated failures.</summary>
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login(
            [FromBody] LoginDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(response, "Login successful."));
        }

        /// <summary>Exchange a refresh token for a new access token + new refresh token (rotation).</summary>
        [HttpPost("refresh")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Refresh(
            [FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(response, "Token refreshed."));
        }

        /// <summary>Revoke one refresh token (log out this device).</summary>
        [HttpPost("logout")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ApiResponseDto<object?>>> Logout(
            [FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "Logged out."));
        }

        /// <summary>Revoke every refresh token of the current user (log out all devices).</summary>
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<ActionResult<ApiResponseDto<object?>>> LogoutAll(CancellationToken cancellationToken)
        {
            await _authService.LogoutAllAsync(cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "Logged out from all devices."));
        }

        /// <summary>Get the current authenticated user.</summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var user = await _authService.GetCurrentUserAsync(cancellationToken);
            return Ok(ApiResponseDto<UserDto>.Ok(user));
        }

        /// <summary>Update the current user's profile (first name, last name, phone). Only sent fields change.</summary>
        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateProfile(
            [FromBody] UpdateUserDto request, CancellationToken cancellationToken)
        {
            var user = await _authService.UpdateProfileAsync(request, cancellationToken);
            return Ok(ApiResponseDto<UserDto>.Ok(user, "Profile updated successfully."));
        }

        /// <summary>Change the password. All other sessions (refresh tokens) are signed out.</summary>
        [Authorize]
        [HttpPost("change-password")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ApiResponseDto<object?>>> ChangePassword(
            [FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
        {
            await _authService.ChangePasswordAsync(request, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "Password changed successfully. Please log in again on your other devices."));
        }

        /// <summary>Confirm the email address using the token sent by email.</summary>
        [HttpPost("confirm-email")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ApiResponseDto<object?>>> ConfirmEmail(
            [FromBody] ConfirmEmailDto request, CancellationToken cancellationToken)
        {
            await _authService.ConfirmEmailAsync(request, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "Email confirmed successfully."));
        }

        /// <summary>Send the confirmation email again. Always answers the same, to avoid revealing which emails exist.</summary>
        [HttpPost("resend-confirmation")]
        [EnableRateLimiting("email")]
        public async Task<ActionResult<ApiResponseDto<object?>>> ResendConfirmation(
            [FromBody] EmailRequestDto request, CancellationToken cancellationToken)
        {
            await _authService.ResendConfirmationEmailAsync(request.Email, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "If the account exists and is not confirmed, a new email has been sent."));
        }

        /// <summary>Start a password reset. Always answers the same, to avoid revealing which emails exist.</summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("email")]
        public async Task<ActionResult<ApiResponseDto<object?>>> ForgotPassword(
            [FromBody] EmailRequestDto request, CancellationToken cancellationToken)
        {
            await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "If an account with this email exists, a reset link has been sent."));
        }

        /// <summary>Set a new password using the token sent by email.</summary>
        [HttpPost("reset-password")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ApiResponseDto<object?>>> ResetPassword(
            [FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(request, cancellationToken);
            return Ok(ApiResponseDto<object?>.Ok(null, "Password has been reset. You can log in now."));
        }
    }
}
