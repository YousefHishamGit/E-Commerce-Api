using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Orders
{
    public class CreateOrderDto
    {
        /// <summary>
        /// Optional: Redis basket key. If not provided, the authenticated user's basket is used automatically.
        /// </summary>
        public string? BasketId { get; set; }

        [Required(ErrorMessage = "DeliveryAddressId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid delivery address.")]
        public int DeliveryAddressId { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required (e.g. CashOnDelivery, CreditCard, PayPal, Wallet).")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        public string? CouponCode { get; set; }
    }
}
