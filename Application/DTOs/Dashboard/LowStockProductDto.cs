namespace Application.DTOs.Dashboard
{
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}
