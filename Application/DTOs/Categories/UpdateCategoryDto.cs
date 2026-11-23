using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Categories
{
    public class UpdateCategoryDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
