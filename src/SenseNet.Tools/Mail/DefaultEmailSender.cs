using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SenseNet.Tools.Mail
{
    internal class DefaultEmailSender : IEmailSender
    {
        private readonly ILogger<DefaultEmailSender> _logger;
        private readonly EmailOptions _options;

        public DefaultEmailSender(IOptions<EmailOptions> options, ILogger<DefaultEmailSender> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task SendAsync(string email, string name, string subject, string message, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            return SendAsync(new EmailData
            {
                ToAddresses = new[] { new EmailAddress(email, name) },
                Subject = subject,
                Body = message
            }, cancel);
        }

        public async Task SendAsync(EmailData emailData, CancellationToken cancel)
        {
            if (emailData == null)
                throw new ArgumentNullException(nameof(emailData));
            if (!(emailData.ToAddresses?.Any() ?? false))
                throw new ArgumentException("No recipient address is specified.", nameof(emailData));

            if (string.IsNullOrEmpty(_options.Server))
                throw new InvalidOperationException("No SMTP server is configured.");

            _logger.LogTrace($"Sending email to {emailData.GetAddresses()}. " +
                              $"Subject: {emailData.Subject}, Server: {_options.Server}");

            try
            {
                // fallback to global options if local sender is not provided
                var senderName = string.IsNullOrEmpty(emailData.FromName)
                    ? _options.SenderName
                    : emailData.FromName;
                var fromAddress = string.IsNullOrEmpty(emailData.FromAddress)
                    ? _options.FromAddress
                    : emailData.FromAddress;

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(senderName, fromAddress));
                mimeMessage.To.AddRange(emailData.ToAddresses?.Select(ea => new MailboxAddress(ea.Name, ea.Address)));
                mimeMessage.Subject = emailData.Subject;
                mimeMessage.Body = new TextPart("html")
                {
                    Text = emailData.Body
                };

                using var client = new SmtpClient
                {
                    // accept all SSL certificates (in case the server supports STARTTLS)
                    ServerCertificateValidationCallback = (_, _, _, _) => true
                };
                
                await client.ConnectAsync(_options.Server, _options.Port, cancellationToken: cancel).ConfigureAwait(false);

                // Note: only needed if the SMTP server requires authentication
                if (!string.IsNullOrEmpty(_options.Username))
                    await client.AuthenticateAsync(_options.Username, _options.Password, cancel).ConfigureAwait(false);

                await client.SendAsync(mimeMessage, cancel).ConfigureAwait(false);
                await client.DisconnectAsync(true, cancel).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    $"Error sending email message to {emailData.GetAddresses()}. {ex.Message}");
            }
        }
    }
}
