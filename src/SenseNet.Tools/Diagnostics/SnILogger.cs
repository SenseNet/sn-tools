using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Routes all log messages to the official .Net log interface.
    /// </summary>
    public class SnILogger : SnEventloggerBase
    {
        private readonly ILogger<SnILogger> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnILogger"/> class.
        /// </summary>
        /// <param name="logger">Target logger service</param>
        public SnILogger(ILogger<SnILogger> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
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
