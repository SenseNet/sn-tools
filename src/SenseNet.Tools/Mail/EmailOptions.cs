namespace SenseNet.Tools.Mail
{
    /// <summary>
    /// Email options for sending emails.
    /// </summary>
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
    }
}
