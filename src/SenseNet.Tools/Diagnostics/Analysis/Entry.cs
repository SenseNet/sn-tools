using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#pragma warning disable 1591

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// Represents a line in the trace file.
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Field index helper
        /// </summary>
        public enum Field
        {
            /// <summary>Value = 0</summary>
            LineId = 0,
            /// <summary>Value = 1</summary>
            Time,
            /// <summary>Value = 2</summary>
            Category,
            /// <summary>Value = 3</summary>
            AppDomain,
            /// <summary>Value = 4</summary>
            ThreadId,
            /// <summary>Value = 5</summary>
            OpId,
            /// <summary>Value = 6</summary>
            Status,
            /// <summary>Value = 7</summary>
            Duration,
            /// <summary>Value = 8</summary>
            Message
        }

        /// <summary>
        /// True if this line is the first in the block that written to disk in one step.
        /// </summary>
        public bool BlockStart;
        /// <summary>
        /// Identifier number of the line. Unique is in the AppDomain.
        /// </summary>
        public int LineId;
        /// <summary>
        /// Creation time of the line.
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// Trace category
        /// </summary>
        public string Category;
        /// <summary>
        /// AppDomain name
        /// </summary>
        public string AppDomain;
        /// <summary>
        /// Current thread id.
        /// </summary>
        public int ThreadId;
        /// <summary>
        /// Id of the operation
        /// </summary>
        public int OpId;
        /// <summary>
        /// Value can be empty, "Start", "End", "UNTERMINATED" or "ERROR"
        /// </summary>
        public string Status;
        /// <summary>
        /// Duration if this line is the end of an operation
        /// </summary>
        public TimeSpan Duration;
        /// <summary>
        /// The subject of the line
        /// </summary>
        public string Message;
        /// <summary>
        /// Original line data.
        /// </summary>
        public string Raw;

        public Dictionary<string, Entry> Associations { get; set; }

        private Entry() { }
        public Entry(Entry sourceEntry)
        {
            CopyPropertiesFrom(sourceEntry);
        }

        /// <summary>
        /// Creates an SnTraceEntry from one line of the trace file
        /// </summary>
        public static Entry Parse(string oneLine)
        {
            // 0        1                           2       3           4       5       6   7               8
            // >11929	2016-04-07 01:59:57.42589	Index	A:/LM..231	T:46	Op:2743	End	00:00:00.000000	IAQ: A160064 EXECUTION.

            if (string.IsNullOrEmpty(oneLine))
                return null;
            if (oneLine.StartsWith("--") || oneLine.StartsWith("MaxPdiff:", StringComparison.OrdinalIgnoreCase))
                return null;

            var data = oneLine.Split('\t');
            if (data.Length < (int)Field.Message)
                return null;

            return new Entry
            {
                Raw = oneLine,
                BlockStart = ParseBlockStart(data[(int)Field.LineId]),
                LineId = ParseLineId(data[(int)Field.LineId]),
                Time = ParseTime(data[(int)Field.Time]),
                Category = data[(int)Field.Category],
                AppDomain = ParseAppDomain(data[(int)Field.AppDomain]),
                ThreadId = ParseThread(data[(int)Field.ThreadId]),
                OpId = ParseOperationId(data[(int)Field.OpId]),
                Status = data[(int)Field.Status],
                Duration = ParseDuration(data[(int)Field.Duration]),
                Message = string.Join("\t", data.Skip((int)Field.Message))
            };
        }

        private static bool ParseBlockStart(string src)
        {
            if (string.IsNullOrEmpty(src))
                return false;
            return src[0] == '>';
        }
        private static int ParseLineId(string src)
        {
            if (string.IsNullOrEmpty(src))
                return 0;
            if (src.StartsWith(">"))
                src = src.Substring(1);
            return int.Parse(src);
        }
        private static DateTime ParseTime(string src)
        {
            return DateTime.Parse(src, CultureInfo.InvariantCulture);
        }
        private static string ParseAppDomain(string src)
        {
            return src.StartsWith("A:") ? src.Substring(2) : src;
        }
        private static int ParseThread(string src)
        {
            if (src.StartsWith("T:"))
                src = src.Substring(2);
            return src.Length == 0 ? default : int.Parse(src);
        }
        private static int ParseOperationId(string src)
        {
            if (src.StartsWith("Op:"))
                src = src.Substring(3);
            return src.Length == 0 ? default : int.Parse(src);
        }
        private static TimeSpan ParseDuration(string src)
        {
            return src.Length == 0 ? default : TimeSpan.Parse(src, CultureInfo.InvariantCulture);
        }

        protected void CopyPropertiesFrom(Entry fromEntry)
        {
            BlockStart = fromEntry.BlockStart;
            LineId = fromEntry.LineId;
            Time = fromEntry.Time;
            Category = fromEntry.Category;
            AppDomain = fromEntry.AppDomain;
            ThreadId = fromEntry.ThreadId;
            OpId = fromEntry.OpId;
            Status = fromEntry.Status;
            Duration = fromEntry.Duration;
            Message = fromEntry.Message;
            Raw = fromEntry.Raw;
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return Raw;
        }

    }
}
