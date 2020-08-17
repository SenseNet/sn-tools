using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Routes all log messages to the official .Net log interface.
    /// </summary>
    public class SnILogger : SnEventloggerBase
    {
        private readonly ILogger<SnILogger> _logger;
        public SnILogger(ILogger<SnILogger> logger)
        {
            _logger = logger;
        }

        protected override void WriteEntry(string entry, EventLogEntryType entryType, int eventId)
        {
            switch (entryType)
            {
                case EventLogEntryType.Error:
                case EventLogEntryType.FailureAudit:
                    _logger?.LogError(eventId, entry);
                    break;
                case EventLogEntryType.SuccessAudit:
                case EventLogEntryType.Information:
                    _logger?.LogInformation(eventId, entry);
                    break;
                case EventLogEntryType.Warning:
                    _logger?.LogWarning(eventId, entry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null);
            }
        }
    }
}
