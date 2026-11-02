using System.Collections.Generic;

namespace Application.DTOs.Basket
{
    public class BasketDto
    {
        public string Id { get; set; } = null!;
        public List<BasketItemDto> Items { get; set; } = new();
        public int? DeliveryMethodId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? OfferDiscount { get; set; }
        public string? CouponCode { get; set; }
        public decimal? CouponDiscount { get; set; }
        public decimal? TaxesCost { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
