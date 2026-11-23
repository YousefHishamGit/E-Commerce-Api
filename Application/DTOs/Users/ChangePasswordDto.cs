using Application.Validation;
using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Users
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;
        [Required, StrongPassword]
        public string NewPassword { get; set; } = null!;
        [Required, Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
