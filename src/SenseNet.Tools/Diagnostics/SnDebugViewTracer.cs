using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// An ISnTracer implementation that enables monitoring the SnTrace output
    /// (verbose log of sensenet) using a debug viewer of your choice - for example 
    /// DebugView in the SysInternals package.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class SnDebugViewTracer : ISnTracer
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
