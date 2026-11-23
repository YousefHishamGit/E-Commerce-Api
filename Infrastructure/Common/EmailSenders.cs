using Application.Contracts.Services.Common;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Common
{
    /// <summary>Sends email through SMTP using MailKit.</summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> settings) => _settings = settings.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port,
                _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }

    /// <summary>Fallback when SMTP is not configured: nothing is sent, the email is only logged.</summary>
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;
        private readonly EmailSettings _settings;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger, IOptions<EmailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (_settings.LogBodies)
                _logger.LogInformation("[EMAIL not sent - SMTP not configured] To: {To} | Subject: {Subject}\n{Body}", toEmail, subject, htmlBody);
            else
                _logger.LogWarning("Email '{Subject}' to {To} was NOT sent: SMTP is not configured (Email:Host).", subject, toEmail);

            return Task.CompletedTask;
        }
    }
}
