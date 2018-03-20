using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SenseNet.Tools;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Main logger API for writing events on different levels (e.g. Warning, Error).
    /// </summary>
    public static class SnLog
    {
        private const int DefaultEventId = 1;

        private static readonly IDictionary<string, object> EmptyProperties = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
        private const int DefaultPriority = -1;
        private static readonly string[] AuditCategory = { "Audit" };

        /// <summary>
        /// Logger instance used by the dedicated static methods. Set this property
        /// once when your application starts.
        /// </summary>
        public static IEventLogger Instance { get; set; } = new TraceEventLogger();

        /// <summary>
        /// Writes an exception to the log. All the inner exceptions will be extracted and logged too.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Event message.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="categories">List of event categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Event properties.</param>
        public static void WriteException(
            Exception exception,
            string message = null,
            int eventId = DefaultEventId,
            IEnumerable<string> categories = null,
            int priority = DefaultPriority,
            string title = null,
            IDictionary<string, object> properties = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (eventId == DefaultEventId)
                eventId = GetEventId(exception);
            var props = GetPropertiesFromException(exception, properties ?? new Dictionary<string, object>());

            var eventType = GetEventType(exception);

            var msg = message == null
                ? exception.Message
                : message + Environment.NewLine + exception.Message;
            Write(eventType, msg, categories, priority, eventId, title, props);
        }

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="message">Event message.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="categories">List of event categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Event properties.</param>
        public static void WriteError(
            object message,
            int eventId = DefaultEventId,
            IEnumerable<string> categories = null,
            int priority = DefaultPriority,
            string title = null,
            IDictionary<string, object> properties = null)
        {
            Write(TraceEventType.Error, message, categories, priority, eventId, title, properties);
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="message">Event message.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="categories">List of event categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Event properties.</param>
        public static void WriteWarning(
            object message,
            int eventId = DefaultEventId,
            IEnumerable<string> categories = null,
            int priority = DefaultPriority,
            string title = null,
            IDictionary<string, object> properties = null)
        {
            Write(TraceEventType.Warning, message, categories, priority, eventId, title, properties);
        }

        /// <summary>
        /// Writes an information message to the log.
        /// </summary>
        /// <param name="message">Event message.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="categories">List of event categories.</param>
        /// <param name="priority">Event priority.</param>
        /// <param name="title">Event title.</param>
        /// <param name="properties">Event properties.</param>
        public static void WriteInformation(
            object message,
            int eventId = DefaultEventId,
            IEnumerable<string> categories = null,
            int priority = DefaultPriority,
            string title = null,
            IDictionary<string, object> properties = null)
        {
            Write(TraceEventType.Information, message, categories, priority, eventId, title, properties);
        }

        /// <summary>
        /// Writes an audit message to the log. Event level will be Verbose; it is 
        /// the responsibility of the logger to channel this event to the appropriate 
        /// log storage based on the provided category.
        /// </summary>
        /// <param name="auditEvent">An object that represents an audit event.</param>
        /// <param name="properties">Event properties.</param>
        public static void WriteAudit(
            IAuditEvent auditEvent,
            IDictionary<string, object> properties = null)
        {
            Write(TraceEventType.Verbose, auditEvent.Message, AuditCategory,
                eventId: auditEvent.EventId, title: auditEvent.Title, properties: properties);
        }

        private static void Write(
            TraceEventType severity,
            object message,
            IEnumerable<string> categories = null,
            int priority = DefaultPriority,
            int eventId = 0,
            string title = null,
            IDictionary<string, object> properties = null)
        {
            Instance.Write(message, new List<string>(categories ?? new string[0]), priority, eventId, severity,
                title ?? string.Empty, properties ?? EmptyProperties);
        }

        private static TraceEventType GetEventType(Exception e)
        {
            var ee = e;
            while (ee != null)
            {
                var eventTypeProvider = ee as IEventTypeProvider;
                if (eventTypeProvider != null)
                    return eventTypeProvider.EventType;
                ee = ee.InnerException;
            }
            return TraceEventType.Error;
        }

        private const string EventIdKey = "EventId";
        internal static int GetEventId(Exception e)
        {
            while (e != null)
            {
                if (e is SnException eb)
                    return eb.ErrorNumber;

                if (e.Data.Contains(EventIdKey))
                {
                    var eventIdObject = e.Data[EventIdKey];
                    if (eventIdObject != null)
                    {
                        if (int.TryParse(eventIdObject.ToString(), out var eventId))
                            return eventId;
                    }
                    return DefaultEventId;
                }
                e = e.InnerException;
            }
            return DefaultEventId;
        }

        private static IDictionary<string, object> GetPropertiesFromException(Exception e, IDictionary<string, object> props)
        {
            props.Add("Messages", Utility.CollectExceptionMessages(e));
            var epath = string.Empty;
            while (e != null)
            {
                epath += e.GetType().Name + "/";
                var data = e.Data;
                foreach (var key in data.Keys)
                    props.Add(epath + key, data[key]);

                var rtle = e as ReflectionTypeLoadException;
                if (rtle != null)
                    props.Add("Types", string.Join(", ", rtle.Types.Select(x => x.FullName)));

                e = e.InnerException;
            }
            return props;
        }
    }
}
