using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Categories
{
    public class CreateCategoryDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
