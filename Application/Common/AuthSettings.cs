namespace Application.Common
{
    /// <summary>Authentication behaviour. Bound from the "Auth" configuration section.</summary>
    public class AuthSettings
    {
        public int RefreshTokenDays { get; set; } = 7;
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
        public int EmailConfirmationHours { get; set; } = 24;
        public int PasswordResetMinutes { get; set; } = 60;

        /// <summary>Base URL of the storefront; links in emails point here, e.g. https://shop.example.com</summary>
        public string FrontendUrl { get; set; } = "http://localhost:3000";
    }
}
