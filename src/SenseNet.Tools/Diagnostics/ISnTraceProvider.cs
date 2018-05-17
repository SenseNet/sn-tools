namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Defines an API for verbose logging / tracing implementations.
    /// </summary>
    public interface ISnTraceProvider
    {
        /// <summary>
        /// Writes a single line message.
        /// </summary>
        void Write(string line);
        /// <summary>
        /// Forces the emptying of all internal buffers.
        /// </summary>
        void Flush();
    }
}
