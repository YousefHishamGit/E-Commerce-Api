using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Users
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 2)]
        public string? FirstName { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string? LastName { get; set; }
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone must be 10-15 digits (optional leading +).")]
        public string? Phone { get; set; }
    }
}
