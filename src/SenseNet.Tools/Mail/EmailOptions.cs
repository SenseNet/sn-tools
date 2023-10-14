using System.Net.Security;
using SenseNet.Tools.Configuration;

namespace SenseNet.Tools.Mail
{
    /// <summary>
    /// Email options for sending emails.
    /// </summary>
    [OptionsClass(sectionName: "Email")]
    public class EmailOptions
    {
        /// <summary>
        /// Mail server address.
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// Mail server port.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Default sender address.
        /// </summary>
        public string FromAddress { get; set; }
        /// <summary>
        /// Default sender name.
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// Username for authentication if required.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password for authentication if required.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Optional SSL certificate validation callback. Developers may
        /// use this to accept all certificates (by always returning true)
        /// in a development environment.
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
    }
}
