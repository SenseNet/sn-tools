using System;
using SenseNet.Diagnostics;

namespace SenseNet.Configuration
{
    /// <summary>
    /// The exception that is thrown when a configuration-related error occurs.
    /// </summary>
    [Serializable]
    public class SnConfigurationException : SnException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnConfigurationException"/> class.
        /// </summary>
        public SnConfigurationException():base(EventId.Configuration) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SnConfigurationException"/> class.
        /// </summary>
        public SnConfigurationException(string message) : base(EventId.Configuration, message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SnConfigurationException"/> class.
        /// </summary>
        public SnConfigurationException(string message, Exception inner) : base(EventId.Configuration, message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnConfigurationException"/> class.
        /// </summary>
        protected SnConfigurationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
