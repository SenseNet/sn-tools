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

        /// <summary>
        /// Initializes a new instance of the <see cref="SnILoggerTracer"/> class.
        /// </summary>
        /// <param name="logger">Target logger service</param>
        public SnILoggerTracer(ILogger<SnILoggerTracer> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void Write(string line)
        {
            _logger?.LogTrace(line);
        }

        /// <inheritdoc />
        public void Flush()
        {
            // do nothing
        }
    }
}
