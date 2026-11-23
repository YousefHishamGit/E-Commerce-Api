using Application.DTOs.Payments;

namespace Application.Contracts.Services
{
    public enum WebhookResult
    {
        /// <summary>Signature valid; event handled (or intentionally ignored).</summary>
        Processed,
        /// <summary>Signature missing or wrong. Respond 4xx so nothing is trusted.</summary>
        InvalidSignature,
        /// <summary>Body is not a transaction notification.</summary>
        Ignored
    }

    public interface IPaymentService
    {
        Task<PayOrderResponseDto> PayOrderAsync(Guid orderId, CancellationToken ct = default);
        Task<WebhookResult> HandlePaymobWebhookAsync(string rawBody, string? hmac, CancellationToken ct = default);
        Task RefundPaymentAsync(int paymentId, CancellationToken ct = default);

        /// <summary>Best-effort full refund request for a completed payment. Never throws; returns false on failure.</summary>
        Task<bool> TryRefundAsync(Domain.Entities.Payment payment, CancellationToken ct = default);
    }
}
