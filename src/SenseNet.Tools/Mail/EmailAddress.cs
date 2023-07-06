namespace SenseNet.Tools.Mail
{
    /// <summary>
    /// Represents an email address.
    /// </summary>
    public class EmailAddress
    {
        /// <summary>
        /// Email address.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the EmailAddress class.
        /// </summary>
        /// <param name="address">Email address.</param>
        /// <param name="name">Display name.</param>
        public EmailAddress(string address, string name = null)
        {
            Address = address;
            Name = name;
        }
    }
}
