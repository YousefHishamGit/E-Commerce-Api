namespace Application.DTOs.Payments
{
    public class PayOrderResponseDto
    {
        public int PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        /// <summary>Send the customer's browser to this URL to enter card / wallet details on Paymob's hosted page.</summary>
        public string CheckoutUrl { get; set; } = null!;
    }
}
