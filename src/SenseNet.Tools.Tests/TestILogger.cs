using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using EventId = Microsoft.Extensions.Logging.EventId;

namespace SenseNet.Tools.Tests
{
    internal class LogEntry
    {
        internal LogLevel LogLevel { get; set; }
        internal EventId EventId { get; set; }
        internal string Message { get; set; }
    }
    internal class TestILogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = new List<LogEntry>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Entries.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = formatter?.Invoke(state, exception)
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
