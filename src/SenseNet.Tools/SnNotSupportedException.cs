using System;
using SenseNet.Diagnostics;

namespace SenseNet
{
    /// <summary>
    /// The exception that is thrown when a feature or operation is not supported.
    /// </summary>
    [Serializable]
    public class SnNotSupportedException : SnException
    {
        private static readonly string DefaultMessage = "Not supported in this version.";

        /// <summary>
        /// Initializes a new instance of the SnNotSupportedException class.
        /// </summary>
        public SnNotSupportedException() : base(EventId.NotSupported, DefaultMessage) { }
        /// <summary>
        /// Initializes a new instance of the SnNotSupportedException class.
        /// </summary>
        public SnNotSupportedException(string message) : base(EventId.NotSupported, message) { }
        /// <summary>
        /// Initializes a new instance of the SnNotSupportedException class.
        /// </summary>
        public SnNotSupportedException(string message, Exception inner) : base(EventId.NotSupported, message, inner) { }

        /// <inheritdoc />
        protected SnNotSupportedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
