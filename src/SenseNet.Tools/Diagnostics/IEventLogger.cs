using System.Collections.Generic;
using System.Diagnostics;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Defines an API for logger implementations for writing general event messages. 
    /// For verbose logging please use SnTrace instead.
    /// </summary>
    public interface IEventLogger
    {
        /// <summary>
        /// Writes a message and its properties to the log.
        /// </summary>
        /// <param name="message">A message to log.</param>
        /// <param name="categories">List of log categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="severity">Event type.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Event properties.</param>
        void Write(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title,
            IDictionary<string, object> properties);
    }
}
