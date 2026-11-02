namespace Application.DTOs.Payments
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string Method { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? MaskedCardNumber { get; set; }
        public DateTime? PaidAt { get; set; }
        public Guid OrderId { get; set; }
    }
}
