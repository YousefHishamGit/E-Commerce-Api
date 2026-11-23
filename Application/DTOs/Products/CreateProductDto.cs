using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Products
{
    public class CreateProductDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Range(0, 1000000, ErrorMessage = "Stock cannot be negative.")]
        public int StockQuantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "A valid CategoryId is required.")]
        public int CategoryId { get; set; }
    }
}
