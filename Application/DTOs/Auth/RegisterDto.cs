using Application.Validation;
using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;
        [Required, StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;
        [Required, RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone must be 10-15 digits (optional leading +).")]
        public string Phone { get; set; } = null!;
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;
        [Required, StrongPassword]
        public string Password { get; set; } = null!;
        [Required, Compare(nameof(Password), ErrorMessage = "Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        [Required, RegularExpression("(?i)^(male|female)$", ErrorMessage = "Gender must be Male or Female.")]
        public string Gender { get; set; } = null!; // "Male" or "Female"
    }
}
