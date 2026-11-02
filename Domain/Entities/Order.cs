using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Order : BaseEntity<Guid>
    {
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Total price (computed / stored)
        public decimal TotalPrice { get; set; }

        // Shipping address snapshot — captured at checkout so future address edits don't affect past orders
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string? ShippingLocationUrl { get; set; }

        // Optional: applied coupon
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        // FK
        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;

        // One-to-Many
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
