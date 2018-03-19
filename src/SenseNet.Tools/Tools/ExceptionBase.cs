using System;
using SenseNet.Diagnostics;

namespace SenseNet.Tools
{
    /// <summary>
    /// Base class for custom exceptions. Contains a general error identifier that
    /// will be recognized by <see cref="SnLog"/> write operations.
    /// </summary>
    public abstract class ExceptionBase : Exception
    {
        /// <summary>
        /// Gets the identifier of the error. <see cref="SnLog"/> uses this value when writing to the log.
        /// </summary>
        public int ErrorNumber { get; }
        /// <summary>
        /// Gets a unique error identifier containing the type name and the error number.
        /// </summary>
        public string ErrorToken => string.Concat(GetType().FullName, ".", ErrorNumber);

        /// <summary>
        /// Initializes a new instance of the ExceptionBase class.
        /// </summary>
        /// <param name="errorNumber">Error identifier.</param>
        protected ExceptionBase(int errorNumber)
        {
            ErrorNumber = errorNumber;
        }
        /// <summary>
        /// Initializes a new instance of the ExceptionBase class.
        /// </summary>
        /// <param name="errorNumber">Error identifier.</param>
        /// <param name="message">Error text.</param>
        protected ExceptionBase(int errorNumber, string message) : base(message)
        {
            ErrorNumber = errorNumber;
        }
        /// <summary>
        /// Initializes a new instance of the ExceptionBase class.
        /// </summary>
        /// <param name="errorNumber">Error identifier.</param>
        /// <param name="message">Error text.</param>
        /// <param name="inner">Inner exception.</param>
        protected ExceptionBase(int errorNumber, string message, Exception inner) : base(message, inner)
        {
            ErrorNumber = errorNumber;
        }

        #region Serialization

        /// <summary>
        /// Initializes a new instance of the ExceptionBase class.
        /// </summary>
        protected ExceptionBase(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            ErrorNumber = info.GetInt32("errNo");
        }
        /// <inheritdoc />
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("errNo", ErrorNumber);
        }

        #endregion
    }
}