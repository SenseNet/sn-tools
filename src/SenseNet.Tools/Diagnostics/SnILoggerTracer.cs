using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Routes all trace messages to the official .Net log interface.
    /// </summary>
    public class SnILoggerTracer : ISnTracer
    {
        private readonly ILogger<SnILoggerTracer> _logger;
        public SnILoggerTracer(ILogger<SnILoggerTracer> logger)
        {
            _logger = logger;
        }

        public void Write(string line)
        {
            _logger?.LogTrace(line);
        }

        public void Flush()
        {
            // do nothing
        }
    }
}
