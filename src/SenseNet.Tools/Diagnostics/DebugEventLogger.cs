using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Logger implementation for writing messages to the debug trace.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class DebugEventLogger : TraceEventLogger
    {
        /// <summary>
        /// Writes a message and its properties to the debug trace.
        /// </summary>
        /// <param name="message">A message to log.</param>
        /// <param name="categories">Optional list of log categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="severity">Event type.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Optional list of event properties.</param>
        public override void Write(object message, ICollection<string> categories, int priority, int eventId,
            TraceEventType severity, string title, IDictionary<string, object> properties)
        {
#if DEBUG
            base.Write(message, categories, priority,eventId, severity, title, properties);
#endif
        }

        /// <summary>
        /// Writes a message to the debug trace.
        /// </summary>
        protected override void Write(string msg)
        {
            Debug.WriteLine(msg);
        }
    }
}
