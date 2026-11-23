namespace Infrastructure.Common
{
    /// <summary>SMTP configuration, bound from the "Email" section. Leave Host empty to only log emails.</summary>
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "no-reply@localhost";
        public string FromName { get; set; } = "E-Commerce";

        /// <summary>Development helper: when no SMTP host is set, also write the email body (with links) to the log.</summary>
        public bool LogBodies { get; set; }
    }
}
