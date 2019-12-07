using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Defines an interface for exception types that need to redefine the default
    /// severity (Error) when they are logged. Custom exception classes can implement 
    /// this interface and override the EventType property to return a different
    /// log level - e.g. Warning instead of Error.
    /// </summary>
    public interface IEventTypeProvider
    {
        /// <summary>
        /// Event level when logging a custom exception. Default: TraceEventType.Error
        /// </summary>
        TraceEventType EventType { get; }
    }
}
