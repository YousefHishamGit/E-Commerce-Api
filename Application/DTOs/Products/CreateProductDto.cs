namespace Application.DTOs.Products
{
    public class CreateProductDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
    }
}
