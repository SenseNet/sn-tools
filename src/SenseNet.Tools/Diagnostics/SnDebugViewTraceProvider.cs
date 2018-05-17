using System.Diagnostics;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// An ISnTraceProvider implementation that enables monitoring the SnTrace output
    /// (sensenet's verbose log) with the DebugView tool of the Sysinternals. 
    /// </summary>
    public class SnDebugViewTraceProvider : ISnTraceProvider
    {
        /// <summary>
        /// Writes the given line to the global Trace channel.
        /// </summary>
        public void Write(string line)
        {
            Trace.WriteLine(line, "SnTrace");
        }

        /// <summary>
        /// Does nothing in this implementation.
        /// </summary>
        public void Flush()
        {
            // do nothing
        }
    }
}
