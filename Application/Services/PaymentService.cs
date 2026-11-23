using System.Text.Json;
using Application.Common;
using Application.Contracts.Repository;
using Application.Contracts.Services;
using Application.Contracts.Services.Common;
using Application.DTOs.Payments;
using Application.Exceptions;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentRepository _payments;
        private readonly IPaymentGateway _gateway;
        private readonly ICurrentUser _currentUser;
        private readonly PaymobSettings _settings;
        private readonly AuthSettings _authSettings;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork uow,
            IPaymentRepository payments,
            IPaymentGateway gateway,
            ICurrentUser currentUser,
            PaymobSettings settings,
            AuthSettings authSettings,
            ILogger<PaymentService> logger)
        {
            _uow = uow;
            _payments = payments;
            _gateway = gateway;
            _currentUser = currentUser;
            _settings = settings;
            _authSettings = authSettings;
            _logger = logger;
        }

        // ── Start a payment ──────────────────────────────────────────────────

        public async Task<PayOrderResponseDto> PayOrderAsync(Guid orderId, CancellationToken ct = default)
        {
            var payment = await _payments.GetLatestForCustomerOrderAsync(orderId, _currentUser.UserId)
                          ?? throw new NotFoundException($"Order with ID {orderId} was not found.");
            var order = payment.Order;

            if (!payment.Method.IsOnline())
                throw new BadRequestException("This order is paid on delivery; there is nothing to pay online.");
            if (payment.Status == PaymentStatus.Completed)
                throw new BadRequestException("This order is already paid.");
            if (payment.Status == PaymentStatus.Refunded)
                throw new BadRequestException("This payment was refunded.");
            if (order.Status != OrderStatus.Pending)
                throw new BadRequestException($"Order is {order.Status}; it can no longer be paid.");
            if (order.CreatedAt.AddMinutes(_settings.UnpaidOrderMinutes) < DateTime.UtcNow)
                throw new BadRequestException("The payment window for this order has expired. Please place a new order.");

            if (!_settings.IsConfigured)
                throw new ServiceUnavailableException("Online payments are not available right now.");

            var integrationId = payment.Method == PaymentMethod.Wallet ? _settings.WalletIntegrationId : _settings.CardIntegrationId;
            if (integrationId <= 0)
                throw new ServiceUnavailableException($"Payment method {payment.Method} is not available right now.");

            // A fresh reference per attempt lets a customer retry after a failed payment
            var reference = Guid.NewGuid().ToString("N");
            var shortId = order.Id.ToString()[..8].ToUpperInvariant();

            var result = await _gateway.CreateIntentionAsync(new PaymentIntentRequest(
                AmountCents: ToCents(payment.Amount),
                Currency: _settings.Currency,
                IntegrationId: integrationId,
                Reference: reference,
                Description: $"Order #{shortId}",
                FirstName: order.Customer.FirstName,
                LastName: order.Customer.LastName,
                Email: order.Customer.Email,
                Phone: NormalizePhone(order.Customer.Phone),
                City: order.ShippingCity,
                Street: order.ShippingStreet,
                RedirectionUrl: $"{_authSettings.FrontendUrl.TrimEnd('/')}/orders/{order.Id}/payment-result"), ct);

            payment.GatewayReference = reference;
            payment.GatewayIntentionId = result.IntentionId;
            payment.Status = PaymentStatus.Pending;
            payment.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            return new PayOrderResponseDto
            {
                PaymentId = payment.Id,
                OrderId = order.Id,
                Amount = payment.Amount,
                Currency = _settings.Currency,
                CheckoutUrl = result.CheckoutUrl
            };
        }

        // ── Webhook ──────────────────────────────────────────────────────────

        public async Task<WebhookResult> HandlePaymobWebhookAsync(string rawBody, string? hmac, CancellationToken ct = default)
        {
            JsonDocument doc;
            try { doc = JsonDocument.Parse(rawBody); }
            catch (JsonException) { return WebhookResult.Ignored; }

            using (doc)
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty("type", out var type) || type.GetString() != "TRANSACTION"
                    || !root.TryGetProperty("obj", out var obj) || obj.ValueKind != JsonValueKind.Object)
                    return WebhookResult.Ignored;

                // Nothing below is trusted until the signature checks out
                if (!PaymobWebhook.Verify(obj, _settings.HmacSecret, hmac))
                {
                    _logger.LogWarning("Paymob webhook rejected: invalid or missing HMAC.");
                    return WebhookResult.InvalidSignature;
                }

                await ApplyTransactionAsync(PaymobWebhook.Parse(obj), ct);
                return WebhookResult.Processed;
            }
        }

        private async Task ApplyTransactionAsync(PaymobTransaction tx, CancellationToken ct)
        {
            if (tx.MerchantReference is null)
            {
                _logger.LogWarning("Paymob transaction {TransactionId} has no merchant reference; ignored.", tx.TransactionId);
                return;
            }

            var payment = await _payments.GetByGatewayReferenceAsync(tx.MerchantReference);
            if (payment is null)
            {
                _logger.LogWarning("Paymob transaction {TransactionId}: no payment for reference {Reference}.", tx.TransactionId, tx.MerchantReference);
                return;
            }

            // Refund / void notifications
            if (tx.IsRefunded || tx.IsVoided)
            {
                if (tx.Success && payment.Status == PaymentStatus.Completed)
                {
                    if (tx.AmountCents >= ToCents(payment.Amount))
                    {
                        payment.Status = PaymentStatus.Refunded;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _uow.SaveChangesAsync();
                        _logger.LogInformation("Payment {PaymentId} refunded (transaction {TransactionId}).", payment.Id, tx.TransactionId);
                    }
                    else
                    {
                        _logger.LogWarning("Payment {PaymentId}: partial refund of {Cents} cents noted but status unchanged.", payment.Id, tx.AmountCents);
                    }
                }
                return;
            }

            if (tx.Pending) return;   // wait for the final notification

            if (tx.Success)
            {
                if (payment.Status == PaymentStatus.Completed) return;   // duplicate delivery

                // Never trust the gateway payload over our own records
                if (tx.AmountCents != ToCents(payment.Amount)
                    || (tx.Currency is not null && !string.Equals(tx.Currency, _settings.Currency, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogError("Payment {PaymentId}: amount/currency mismatch (expected {Expected} {Currency}, got {Got} {GotCurrency}). Not marked paid.",
                        payment.Id, ToCents(payment.Amount), _settings.Currency, tx.AmountCents, tx.Currency);
                    return;
                }

                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionId = tx.TransactionId;
                payment.MaskedCardNumber = tx.CardLast4;
                payment.UpdatedAt = DateTime.UtcNow;

                var order = payment.Order;
                var paidAfterCancel = order.Status == OrderStatus.Cancelled;
                if (order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Confirmed;
                    order.UpdatedAt = DateTime.UtcNow;
                }

                await _uow.SaveChangesAsync();
                _logger.LogInformation("Payment {PaymentId} for order {OrderId} completed.", payment.Id, order.Id);

                if (paidAfterCancel)
                {
                    // Customer paid at the moment the order expired/was cancelled: give the money back
                    _logger.LogWarning("Order {OrderId} was already cancelled when payment arrived. Refunding.", order.Id);
                    await TryRefundAsync(payment, ct);
                }
                return;
            }

            // Failed attempt. A completed payment is never downgraded by a later failed attempt.
            if (payment.Status != PaymentStatus.Completed && payment.Status != PaymentStatus.Refunded)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
                _logger.LogInformation("Payment {PaymentId} failed (transaction {TransactionId}).", payment.Id, tx.TransactionId);
            }
        }

        // ── Refund ───────────────────────────────────────────────────────────

        public async Task RefundPaymentAsync(int paymentId, CancellationToken ct = default)
        {
            var payment = await _payments.GetByIdWithOrderAsync(paymentId)
                          ?? throw new NotFoundException($"Payment {paymentId} was not found.");

            if (payment.Status != PaymentStatus.Completed || string.IsNullOrEmpty(payment.TransactionId))
                throw new BadRequestException("Only completed online payments can be refunded.");

            if (!await TryRefundAsync(payment, ct))
                throw new ServiceUnavailableException("The payment provider did not accept the refund request. Try again or refund from the Paymob dashboard.");
        }

        /// <summary>Requests a full refund. The payment becomes Refunded when the gateway confirms through the webhook.</summary>
        public async Task<bool> TryRefundAsync(Domain.Entities.Payment payment, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(payment.TransactionId)) return false;
            try
            {
                var accepted = await _gateway.RefundAsync(payment.TransactionId, ToCents(payment.Amount), ct);
                _logger.LogInformation("Refund request for payment {PaymentId} accepted={Accepted}.", payment.Id, accepted);
                return accepted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund request for payment {PaymentId} failed.", payment.Id);
                return false;
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static long ToCents(decimal amount) => (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

        /// <summary>Paymob prefers international format: 01012345678 -> +201012345678.</summary>
        private static string NormalizePhone(string phone)
        {
            phone = phone.Trim();
            return phone.StartsWith('0') && phone.Length == 11 ? "+20" + phone[1..] : phone;
        }
    }
}
