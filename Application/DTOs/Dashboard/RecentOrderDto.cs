namespace Application.DTOs.Dashboard
{
    public class RecentOrderDto
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int ItemsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
