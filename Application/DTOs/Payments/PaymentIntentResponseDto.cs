namespace Application.DTOs.Payments
{
    public class PaymentIntentResponseDto
    {
        public string BasketId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string PaymentIntentId { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
