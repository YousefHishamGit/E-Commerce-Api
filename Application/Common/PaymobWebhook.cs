using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Common
{
    /// <summary>A transaction notification sent by Paymob (the "obj" of a TRANSACTION webhook).</summary>
    public record PaymobTransaction(
        string TransactionId,
        string? MerchantReference,
        long AmountCents,
        string? Currency,
        bool Success,
        bool Pending,
        bool IsRefunded,
        bool IsVoided,
        string? CardLast4);

    /// <summary>Signature verification and parsing for Paymob "transaction processed" webhooks.</summary>
    public static class PaymobWebhook
    {
        /// <summary>Fields concatenated, in this exact order, to compute the HMAC (paths relative to "obj").</summary>
        private static readonly string[] HmacFields =
        {
            "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction",
            "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded",
            "is_standalone_payment", "is_voided", "order.id", "owner", "pending",
            "source_data.pan", "source_data.sub_type", "source_data.type", "success"
        };

        public static string ComputeHmac(JsonElement obj, string secret)
        {
            var sb = new StringBuilder();
            foreach (var path in HmacFields)
                sb.Append(Render(Resolve(obj, path)));

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
        }

        public static bool Verify(JsonElement obj, string secret, string? providedHmac)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(providedHmac))
                return false;

            var expected = Encoding.ASCII.GetBytes(ComputeHmac(obj, secret));
            var provided = Encoding.ASCII.GetBytes(providedHmac.Trim().ToLowerInvariant());

            // constant-time comparison (length mismatch returns false)
            return CryptographicOperations.FixedTimeEquals(expected, provided);
        }

        public static PaymobTransaction Parse(JsonElement obj)
        {
            var pan = Render(Resolve(obj, "source_data.pan"));
            var last4 = new string(pan.Where(char.IsDigit).TakeLast(4).ToArray());

            var merchantRef = Render(Resolve(obj, "order.merchant_order_id"));

            return new PaymobTransaction(
                TransactionId: Render(Resolve(obj, "id")),
                MerchantReference: merchantRef.Length == 0 ? null : merchantRef,
                AmountCents: long.TryParse(Render(Resolve(obj, "amount_cents")), out var cents) ? cents : -1,
                Currency: NullIfEmpty(Render(Resolve(obj, "currency"))),
                Success: Render(Resolve(obj, "success")) == "true",
                Pending: Render(Resolve(obj, "pending")) == "true",
                IsRefunded: Render(Resolve(obj, "is_refunded")) == "true",
                IsVoided: Render(Resolve(obj, "is_voided")) == "true",
                CardLast4: last4.Length == 0 ? null : last4);
        }

        private static string? NullIfEmpty(string s) => s.Length == 0 ? null : s;

        private static JsonElement? Resolve(JsonElement obj, string path)
        {
            var current = obj;
            foreach (var part in path.Split('.'))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                    return null;
            }
            return current;
        }

        /// <summary>Strings as-is, numbers as written, booleans lowercase, null/missing as empty.</summary>
        private static string Render(JsonElement? value)
        {
            if (value is not { } v) return string.Empty;
            return v.ValueKind switch
            {
                JsonValueKind.String => v.GetString() ?? string.Empty,
                JsonValueKind.Number => v.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                _ => v.GetRawText()
            };
        }
    }
}
