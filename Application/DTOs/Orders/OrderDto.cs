using Application.DTOs.Payments;

namespace Application.DTOs.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public int? CouponId { get; set; }
        public string? CouponCode { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;

        // Shipping address snapshot
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string? ShippingLocationUrl { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
