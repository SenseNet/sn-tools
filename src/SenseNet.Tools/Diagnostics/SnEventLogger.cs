using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Built-in, lightweight IEventLogger implementation that writes events into the selected and existing windows event log.
    /// </summary>
    public class SnEventLogger : IEventLogger
    {
        private readonly EventLog _eventLog;

        /// <summary>
        /// Initializes an instance of the SnEventLogger class by passed logName and logSourceName.
        /// The defined log and logsource must be exist.
        /// </summary>
        /// <param name="logName">Name of the log. Cannot be null or empty.</param>
        /// <param name="logSourceName">Name of the log source. Cannot be null or empty.</param>
        public SnEventLogger(string logName, string logSourceName)
        {
            if (logName == null)
                throw new ArgumentNullException(nameof(logName));
            if (logName.Length == 0)
                throw new ArgumentException($"The {nameof(logName)} cannot be empty.");
            if (logSourceName == null)
                throw new ArgumentNullException(nameof(logSourceName));
            if (logSourceName.Length == 0)
                throw new ArgumentException($"The {nameof(logSourceName)} cannot be empty.");

            if (EventLog.SourceExists(logSourceName))
            {
                var existingLogName = EventLog.LogNameFromSourceName(logSourceName, Environment.MachineName);
                if (existingLogName != logName)
                    throw new InvalidOperationException(
                        $"Cannot use the '{logName}' log with the '{logSourceName}' source because this source is used in the '{existingLogName}'");
            }
            else
            {
                EventLog.CreateEventSource(new EventSourceCreationData(logSourceName, logName) { MachineName = "." });
            }

            _eventLog = new EventLog(logName) {Source = logSourceName};
        }

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
        public virtual void Write(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title,
            IDictionary<string, object> properties)
        {
            if (severity == TraceEventType.Verbose || _eventLog == null)
                return;

            EventLogEntryType entryType;
            switch (severity)
            {
                case TraceEventType.Warning:
                    entryType = EventLogEntryType.Warning;
                    break;
                case TraceEventType.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                default:
                    entryType = EventLogEntryType.Information;
                    break;
            }

            _eventLog.WriteEntry(FormatMessage(message, categories, priority, eventId, severity, title, properties), entryType, eventId);
        }

        private readonly string _cr = Environment.NewLine;
        private readonly string _defaultCategories = "General";

        /// <summary>
        /// Returns a formatted string representation of the whole log entry by the passed parameters.
        /// </summary>
        protected virtual string FormatMessage(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            var process = Process.GetCurrentProcess();
            var thread = Thread.CurrentThread;
            return
                $"Timestamp: {DateTime.UtcNow}" + _cr +
                $"Message: {message}" + _cr +
                $"Category: {FormatCategories(categories)}" + _cr +
                $"Priority: {priority}" + _cr +
                $"EventId: {eventId}" + _cr +
                $"Severity: {severity}" + _cr +
                $"Title: {title}" + _cr +
                $"Machine: {Environment.MachineName}" + _cr +
                $"Application Domain: {Environment.UserDomainName}" + _cr +
                $"Process Id: {process.Id}" + _cr +
                $"Process Name: {process.ProcessName}" + _cr +
                $"Managed Thread Id: {thread.ManagedThreadId}" + _cr +
                $"Thread Name: {thread.Name}" + _cr +
                $"Extended Properties: {FormatProperties(properties)}";
        }
        /// <summary>
        /// Returns a formatted string representation of the categories.
        /// Currently it is a comma separated list of the passed categories.
        /// If the list is null or empty, the value is "General".
        /// </summary>
        /// <param name="categories">Category collection. Can be null or empty.</param>
        protected virtual string FormatCategories(ICollection<string> categories)
        {
            return categories == null || categories.Count == 0 ? _defaultCategories : string.Join(", ", categories);
        }
        /// <summary>
        /// Returns a formattedd string representation of the passedd properties.
        /// </summary>
        protected virtual string FormatProperties(IDictionary<string, object> properties)
        {
            return properties == null
                ? string.Empty
                : string.Join(_cr, properties.Select(x => $"{x.Key} - {x.Value}"));
        }
    }

}
