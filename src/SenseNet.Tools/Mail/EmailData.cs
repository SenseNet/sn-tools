using System;
using System.Linq;

namespace SenseNet.Tools.Mail
{
    /// <summary>
    /// Represents an email message.
    /// </summary>
    public class EmailData
    {
        /// <summary>
        /// Sender address if different from the configured default.
        /// </summary>
        public string FromAddress { get; set; }
        /// <summary>
        /// Sender name if different from the configured default.
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// Recipient address.
        /// </summary>
        public EmailAddress[] ToAddresses { get; set; } = Array.Empty<EmailAddress>();
        /// <summary>
        /// Email subject.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Email body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets the first 3 recipient addresses separated by comma. Intended for logging.
        /// </summary>
        internal string GetAddresses()
        {
            if (!(ToAddresses?.Any() ?? false))
                return string.Empty;

            var emails = string.Join(", ", ToAddresses.Select(ea => ea.Address).Take(3));
            if (ToAddresses.Length > 3)
                emails += ", ...";

            return emails;
        }
    }
}
