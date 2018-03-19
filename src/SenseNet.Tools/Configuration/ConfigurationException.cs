using System;
using SenseNet.Diagnostics;
using SenseNet.Tools;

namespace SenseNet.Configuration
{
    /// <summary>
    /// The exception that is thrown when a configuration-related error occurs.
    /// </summary>
    [Serializable]
    public class ConfigurationException : ExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException():base(EventId.Configuration) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException(string message) : base(EventId.Configuration, message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException(string message, Exception inner) : base(EventId.Configuration, message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        protected ConfigurationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
