﻿using System.Diagnostics;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// An ISnTracer implementation that enables monitoring the SnTrace output
    /// (sensenet's verbose log) using a debug viewer of your choice - for example 
    /// DebugView in the Sysinternals package.
    /// </summary>
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
