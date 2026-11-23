using System.ComponentModel.DataAnnotations;
using Application.Validation;

namespace Application.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required, StringLength(200)]
        public string RefreshToken { get; set; } = null!;
    }

    public class EmailRequestDto
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;
    }

    public class ConfirmEmailDto
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;

        [Required, StringLength(200)]
        public string Token { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;

        [Required, StringLength(200)]
        public string Token { get; set; } = null!;

        [Required, StrongPassword]
        public string NewPassword { get; set; } = null!;

        [Required, Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
