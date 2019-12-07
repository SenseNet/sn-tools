// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Represents an audit event.
    /// </summary>
    public interface IAuditEvent
    {
        /// <summary>
        /// Event id.
        /// </summary>
        int EventId { get; }
        /// <summary>
        /// Message to log.
        /// </summary>
        string Message { get; }
        /// <summary>
        /// Event title.
        /// </summary>
        string Title { get; }
    }
}
