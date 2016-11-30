using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Logger implementation for writing messages to the trace.
    /// </summary>
    public class TraceEventLogger : IEventLogger
    {
        /// <summary>
        /// Writes a message and its properties to the trace.
        /// </summary>
        /// <param name="message">A message to log.</param>
        /// <param name="categories">Optional list of log categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="severity">Event type.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Optional list of event properties.</param>
        public virtual void Write(object message, ICollection<string> categories, int priority, int eventId,
            TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            var msg = 
                $@"Message: {message}; Categories:{(categories == null ? string.Empty : string.Join(",", categories))}; " +
                $@"Priority:{priority}; EventId:{eventId}; " +
                $@"Severity: {severity}; Title: {title}; " +
                $@"Properties: {(properties == null
                    ? string.Empty
                    : string.Join(", ", properties.Select(p => string.Concat(p.Key, ":", p.Value))))}";

            Write(msg.Replace('\r', '\n').Replace("\n\n", "\n").Replace("\n", " | "));
        }

        /// <summary>
        /// Writes a message to the trace.
        /// </summary>
        protected virtual void Write(string msg)
        {
            Trace.WriteLine(msg);
        }
    }
}
