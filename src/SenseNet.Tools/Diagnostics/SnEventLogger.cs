using System;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Built-in, lightweight IEventLogger implementation that writes events into the selected and existing windows event log.
    /// </summary>
    public class SnEventLogger : SnEventloggerBase
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

        /// <inheritdoc />
        public override void Write(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title,
            IDictionary<string, object> properties)
        {
            if (_eventLog != null)
                base.Write(message, categories, priority, eventId, severity, title, properties);
        }

        /// <inheritdoc />
        protected override void WriteEntry(string entry, EventLogEntryType entryType, int eventId)
        {
            lock (_eventLog)
                _eventLog.WriteEntry(entry, entryType, eventId);
        }
    }
}
