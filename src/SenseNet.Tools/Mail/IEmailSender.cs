using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SenseNet.Tools.Mail
{
    /// <summary>
    /// Defines methods for sending emails.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="name">Display name</param>
        /// <param name="subject">Email subject</param>
        /// <param name="message">Message body</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param> 
        Task SendAsync(string email, string name, string subject, string message, CancellationToken cancel);

        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="emailData">Email data.</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param> 
        Task SendAsync(EmailData emailData, CancellationToken cancel);
    }

    internal class NullEmailSender : IEmailSender
    {
        private readonly ILogger<NullEmailSender> _logger;

        public NullEmailSender(ILogger<NullEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string email, string name, string subject, string message, CancellationToken cancel)
        {
            _logger.LogTrace("Email sending is disabled. To: {email} Subject: {subject}", email, subject);
            return Task.FromResult(0);
        }

        public Task SendAsync(EmailData emailData, CancellationToken cancel)
        {
            _logger.LogTrace("Email sending is disabled. To: {email} Subject: {subject}",
                emailData?.ToAddresses?.FirstOrDefault()?.Address, emailData?.Subject);

            return Task.FromResult(0);
        }
    }
}
