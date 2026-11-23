using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Common;
using Application.Contracts.Services.Common;
using Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common
{
    /// <summary>Paymob Egypt client: Intention API (Unified Checkout) + refunds. The secret key never leaves the server.</summary>
    public class PaymobGateway : IPaymentGateway
    {
        private readonly HttpClient _http;
        private readonly PaymobSettings _settings;
        private readonly ILogger<PaymobGateway> _logger;

        public PaymobGateway(HttpClient http, PaymobSettings settings, ILogger<PaymobGateway> logger)
        {
            _http = http;
            _settings = settings;
            _logger = logger;

            _http.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            _http.Timeout = TimeSpan.FromSeconds(30);
            if (!string.IsNullOrWhiteSpace(settings.SecretKey))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.SecretKey);
        }

        public async Task<PaymentIntentResult> CreateIntentionAsync(PaymentIntentRequest r, CancellationToken ct = default)
        {
            var body = new Dictionary<string, object?>
            {
                ["amount"] = r.AmountCents,
                ["currency"] = r.Currency,
                ["payment_methods"] = new[] { r.IntegrationId },
                // One line item equal to the whole order, so discounts/shipping/tax never break Paymob's "items must add up" rule
                ["items"] = new[] { new { name = r.Description, amount = r.AmountCents, quantity = 1 } },
                ["billing_data"] = new
                {
                    first_name = r.FirstName,
                    last_name = r.LastName,
                    phone_number = r.Phone,
                    email = r.Email,
                    street = Or(r.Street),
                    building = "NA",
                    apartment = "NA",
                    floor = "NA",
                    city = Or(r.City),
                    state = "NA",
                    country = "EG"
                },
                ["customer"] = new { first_name = r.FirstName, last_name = r.LastName, email = r.Email },
                ["special_reference"] = r.Reference
            };
            if (!string.IsNullOrWhiteSpace(_settings.NotificationUrl)) body["notification_url"] = _settings.NotificationUrl;
            if (!string.IsNullOrWhiteSpace(r.RedirectionUrl)) body["redirection_url"] = r.RedirectionUrl;

            using var response = await _http.PostAsJsonAsync("v1/intention/", body, ct);
            var text = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob create-intention failed: HTTP {Status} {Body}", (int)response.StatusCode, Truncate(text));
                throw new ServiceUnavailableException("The payment provider could not start the payment. Please try again later.");
            }

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var clientSecret = Str(root, "client_secret");
            if (string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Paymob create-intention response had no client_secret: {Body}", Truncate(text));
                throw new ServiceUnavailableException("The payment provider returned an unexpected response.");
            }

            var checkoutUrl = $"{_settings.BaseUrl.TrimEnd('/')}/unifiedcheckout/" +
                              $"?publicKey={Uri.EscapeDataString(_settings.PublicKey)}&clientSecret={Uri.EscapeDataString(clientSecret)}";

            return new PaymentIntentResult(Str(root, "id") ?? string.Empty, Str(root, "intention_order_id"), checkoutUrl);
        }

        public async Task<bool> RefundAsync(string transactionId, long amountCents, CancellationToken ct = default)
        {
            if (!long.TryParse(transactionId, out var txId))
                return false;

            using var response = await _http.PostAsJsonAsync("api/acceptance/void_refund/refund",
                new { transaction_id = txId, amount_cents = amountCents }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Paymob refund failed: HTTP {Status} {Body}", (int)response.StatusCode, Truncate(text));
                return false;
            }
            return true;
        }

        private static string Or(string? s) => string.IsNullOrWhiteSpace(s) ? "NA" : s;
        private static string Truncate(string s) => s.Length <= 500 ? s : s[..500];

        private static string? Str(JsonElement e, string name)
            => e.TryGetProperty(name, out var v)
                ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ValueKind == JsonValueKind.Null ? null : v.GetRawText()
                : null;
    }
}
