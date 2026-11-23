namespace Application.DTOs.Dashboard
{
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
