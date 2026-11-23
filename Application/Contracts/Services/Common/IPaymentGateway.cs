namespace Application.Contracts.Services.Common
{
    public record PaymentIntentRequest(
        long AmountCents,
        string Currency,
        int IntegrationId,
        string Reference,
        string Description,
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        string City,
        string Street,
        string? RedirectionUrl);

    public record PaymentIntentResult(string IntentionId, string? GatewayOrderId, string CheckoutUrl);

    public interface IPaymentGateway
    {
        /// <summary>Creates a payment session and returns the hosted checkout URL the customer must be sent to.</summary>
        Task<PaymentIntentResult> CreateIntentionAsync(PaymentIntentRequest request, CancellationToken ct = default);

        /// <summary>Asks the gateway to refund a captured transaction. Returns true if the request was accepted.</summary>
        Task<bool> RefundAsync(string transactionId, long amountCents, CancellationToken ct = default);
    }
}
