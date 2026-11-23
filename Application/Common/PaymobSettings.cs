namespace Application.Common
{
    /// <summary>Paymob (Egypt) configuration, bound from the "Paymob" section. Keep the keys in user-secrets / env vars.</summary>
    public class PaymobSettings
    {
        public string BaseUrl { get; set; } = "https://accept.paymob.com";

        /// <summary>Secret key (egy_sk_...). Server-side only, used for the Authorization header.</summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>Public key (egy_pk_...). Safe to expose; used to build the checkout URL.</summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>HMAC secret from the Paymob dashboard, used to verify webhooks.</summary>
        public string HmacSecret { get; set; } = string.Empty;

        /// <summary>Integration (payment method) ID used for CreditCard / DebitCard.</summary>
        public int CardIntegrationId { get; set; }

        /// <summary>Integration ID used for mobile Wallet payments.</summary>
        public int WalletIntegrationId { get; set; }

        /// <summary>Public HTTPS URL of POST /api/Payments/paymob/webhook. Leave empty to use the one set in the Paymob dashboard.</summary>
        public string NotificationUrl { get; set; } = string.Empty;

        public string Currency { get; set; } = "EGP";

        /// <summary>Online orders not paid within this many minutes are cancelled and their stock released.</summary>
        public int UnpaidOrderMinutes { get; set; } = 30;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(SecretKey) && !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(HmacSecret);
    }
}
