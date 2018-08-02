using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Abstract base class for defining general event logger behavior.
    /// </summary>
    public abstract class SnEventloggerBase : IEventLogger
    {
        private readonly string _cr = Environment.NewLine;
        private readonly string _defaultCategories = "General";

        /// <inheritdoc />
        public virtual void Write(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title,
            IDictionary<string, object> properties)
        {
            if (severity == TraceEventType.Verbose)
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

            WriteEntry(FormatMessage(message, categories, priority, eventId, severity, title, properties), entryType, eventId);
        }

        /// <summary>
        /// Writes a message to the event log.
        /// </summary>
        /// <param name="entry">Message text.</param>
        /// <param name="entryType">Type of the entry.</param>
        /// <param name="eventId">Event identifier.</param>
        protected abstract void WriteEntry(
            string entry,
            EventLogEntryType entryType,
            int eventId);

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
