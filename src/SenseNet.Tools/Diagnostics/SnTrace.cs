using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Efficient and scalable tracing component. Provides methods for recording 
    /// verbose information about the running system. Collects messages into a buffer 
    /// and writes them to the file system periodically.
    /// This class cannot be inherited.
    /// </summary>
    public static class SnTrace
    {
        //====================================================================== Nested classess

        private static class Config
        {
            public static readonly long BufferSize;
            public static readonly int WriteToFileDelay;
            public static readonly short MaxWritesInOneFile;
            public static readonly int LinesPerTrace;

            private static readonly string[] AvailableSections = { "detailedLogger", "sensenet/detailedLogger" };

            static Config()
            {
                NameValueCollection collection = null;
                foreach (var availableSection in AvailableSections)
                {
                    collection = ConfigurationManager.GetSection(availableSection) as NameValueCollection;
                    if (collection != null)
                        break;
                }

                BufferSize = Parse<long>(collection, "BufferSize", 10000);
                WriteToFileDelay = Parse(collection, "WriteToFileDelay", 1000);
                MaxWritesInOneFile = Parse<short>(collection, "MaxWritesInOneFile", 100);
                LinesPerTrace = Parse(collection, "LinesPerTrace", 1000);
            }

            private static T Parse<T>(NameValueCollection collection, string key, T defaultValue)
            {
                if (collection == null)
                    return defaultValue;

                var value = collection.Get(key);
                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception e)
                {
                    throw new ApplicationException(string.Format("Invalid configuration: key: '{0}', value: '{1}'.", key, value), e);
                }
            }
        }

        /// <summary>
        /// Represents an execution block that needs a start and finish log message
        /// regardless of whether the execution was successful or not. The two
        /// messages can be associated by their common operation id. 
        /// Setting the Successful property to true is mandatory when the execution
        /// was successful - otherwise the logger would assume that the operation failed.
        /// Tipical usage:
        /// using (var op = SnTrace.StartOperation("message")) { ...; op.Successful = true; }
        /// </summary>
        public class Operation : IDisposable
        {
            private static long _nextId = 1;

            internal static readonly Operation Null = new Operation(0L);

            /// <summary>
            /// Gets the operation identifier that is unique in the current AppDomain.
            /// </summary>
            public long Id { get; }

            /// <summary>
            /// Gets the category name.
            /// </summary>
            public string Category { get; }
            /// <summary>
            /// Gets the time when the operation started.
            /// </summary>
            public DateTime StartedAt { get; internal set; }
            /// <summary>
            /// Gets the operation message that is written at start and at the end.
            /// </summary>
            public string Message { get; internal set; }
            /// <summary>
            /// Gets or sets a value indicating whether the operation is finished correctly.
            /// Always set this flag to true when the code block executed correctly.
            /// Default is false.
            /// </summary>
            public bool Successful { get; set; }

            private Operation(long id)
            {
                Id = id;
                Category = string.Empty;
            }
            internal Operation(string category)
            {
                Id = Interlocked.Increment(ref _nextId) - 1;
                Category = category;
            }

            private void Finish()
            {
                if (this != Null)
                    WriteEndToLog(this);
            }

            /// <summary>
            /// Finishes the operation and writes the trace line containing the message and the running time.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// Releases the unmanaged resources used by the Operation object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">True to release both managed and unmanaged resources or false to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                    Finish();
            }
        }

        /// <summary>
        /// Represents an SnTrace category. It helps differentiating trace lines 
        /// that are generated by different features.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public class SnTraceCategory
        {
            /// <summary>
            /// Gets the name of the category.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets or sets whether the category is enabled or not. Messages sent to 
            /// a disabled category will not be writted to the trace log.
            /// </summary>
            public bool Enabled { get; set; }

            internal SnTraceCategory(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Starts a traced operation in the current category. The message will be written 
            /// to the trace with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            /// <returns>A disposable operation object usually encapsulated in a using block.</returns>
            public Operation StartOperation(string message, params object[] args)
            {
                return Enabled ? StartOp(Name, message, args) : Operation.Null;
            }
            /// <summary>
            /// Writes a line to the trace with the current category. The message will be written with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            public void Write(string message, params object[] args)
            {
                if (!Enabled)
                    return;
                Log(Name, false, message, args);
            }
            /// <summary>
            /// Writes an error line to the trace with the current category. The message will be written with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            public void WriteError(string message, params object[] args)
            {
                if (!Enabled)
                    return;
                Log(Name, true, message, args);
            }
        }

        //====================================================================== Built-in categories

        /// <summary>ContentOperation category</summary>
        public static readonly SnTraceCategory ContentOperation = new SnTraceCategory("ContentOperation");
        /// <summary>Database category</summary>
        public static readonly SnTraceCategory Database = new SnTraceCategory("Database");
        /// <summary>Index category</summary>
        public static readonly SnTraceCategory Index = new SnTraceCategory("Index");
        /// <summary>IndexQueue category</summary>
        public static readonly SnTraceCategory IndexQueue = new SnTraceCategory("IndexQueue");
        /// <summary>Query category</summary>
        public static readonly SnTraceCategory Query = new SnTraceCategory("Query");
        /// <summary>Repository category</summary>
        public static readonly SnTraceCategory Repository = new SnTraceCategory("Repository");
        /// <summary>Messaging category</summary>
        public static readonly SnTraceCategory Messaging = new SnTraceCategory("Messaging");
        /// <summary>Security category</summary>
        public static readonly SnTraceCategory Security = new SnTraceCategory("Security");
        /// <summary>SecurityQueue category</summary>
        public static readonly SnTraceCategory SecurityQueue = new SnTraceCategory("SecurityQueue");
        /// <summary>System category</summary>
        public static readonly SnTraceCategory System = new SnTraceCategory("System");
        /// <summary>Web category</summary>
        public static readonly SnTraceCategory Web = new SnTraceCategory("Web");
        /// <summary>Workflow category</summary>
        public static readonly SnTraceCategory Workflow = new SnTraceCategory("Workflow");
        /// <summary>TaskManagement category</summary>
        public static readonly SnTraceCategory TaskManagement = new SnTraceCategory("TaskManagement");
        /// <summary>Test category</summary>
        public static readonly SnTraceCategory Test = new SnTraceCategory("Test");
        /// <summary>Event category</summary>
        public static readonly SnTraceCategory Event = new SnTraceCategory("Event");
        /// <summary>Custom category</summary>
        public static readonly SnTraceCategory Custom = new SnTraceCategory("Custom");

        /// <summary>
        /// Contains all SnTrace categories to help enumerate them.
        /// </summary>
        public static readonly SnTraceCategory[] Categories = { ContentOperation, Database, Index, IndexQueue, Query, Repository, Messaging, Security, SecurityQueue, System, Web, Workflow, TaskManagement, Test, Event, Custom };

        //====================================================================== Static API

        /// <summary>
        /// Creates a dynamic trace category.
        /// </summary>
        /// <param name="name">Category name.</param>
        /// <returns>A category object that is enabled (meaning messages written into it will be persisted) if the Custom category is enabled.</returns>
        public static SnTraceCategory Category(string name)
        {
            return new SnTraceCategory(name) { Enabled = Custom.Enabled };
        }

        /// <summary>
        ///  Starts a traced operation in the "Custom" category. The message will be written to the trace with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        /// <returns></returns>
        public static Operation StartOperation(string message, params object[] args)
        {
            return Custom.StartOperation(message, args);
        }
        /// <summary>
        /// Writes a line to the trace in the "Custom" category. The message will be written with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        public static void Write(string message, params object[] args)
        {
            Custom.Write(message, args);
        }
        /// <summary>
        /// Writes an error line to the trace in the "Custom" category. The message will be written with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        public static void WriteError(string message, params object[] args)
        {
            Custom.WriteError(message, args);
        }

        /// <summary>
        /// Enables all categories.
        /// </summary>
        public static void EnableAll()
        {
            foreach (var snTraceCategory in Categories)
            {
                snTraceCategory.Enabled = true;
            }
        }

        /// <summary>
        /// Disables all categories.
        /// </summary>
        public static void DisableAll()
        {
            foreach (var snTraceCategory in Categories)
            {
                snTraceCategory.Enabled = false;
            }
        }

        //====================================================================== Buffer and Operation

        private static Operation StartOp(string category, string message, params object[] args)
        {
            var op = new Operation(category) {StartedAt = DateTime.UtcNow};
            Log(category, op, message, args);
            return op;
        }
        internal static void Log(string category, bool isError, string message, params object[] args)
        {
            // protection against unprintable characters
            var line = SafeFormatString(category, isError, null, message, args);

            // writing to the buffer
            var p = Interlocked.Increment(ref _bufferPosition) - 1;
            _buffer[p % Config.BufferSize] = line;
        }

        private static void WriteEndToLog(Operation op)
        {
            var line = FinishOperation(op);

            // writing to the buffer
            var p = Interlocked.Increment(ref _bufferPosition) - 1;
            _buffer[p % Config.BufferSize] = line;
        }

        private static readonly string[] _buffer = new string[Config.BufferSize];

        private static long _bufferPosition; // this field is incremented by every logger thread.
        private static long _lastBufferPosition; // this field is written by only CollectLines method.

        /// <summary>Statistical data: the longest gap between p0 and p1</summary>
        private static long _maxPdiff;

        private static string __appDomainName;
        private static string AppDomainName => __appDomainName ?? (__appDomainName = AppDomain.CurrentDomain.FriendlyName);

        /*================================================================== Logger */

        private static void Log(string category, Operation op, string message, params object[] args)
        {
            // protection against unprintable characters
            var line = SafeFormatString(category, false, op, message, args);

            // writing to the buffer
            var p = Interlocked.Increment(ref _bufferPosition) - 1;
            _buffer[p % Config.BufferSize] = line;
        }

        private static string SafeFormatString(string category, bool isError, Operation op, string message, params object[] args)
        {
            var lineCounter = Interlocked.Increment(ref _lineCounter);
            var line = op != null
                ? string.Format("{0}\t{1}\t{2}\tA:{3}\tT:{4}\tOp:{5}\tStart\t\t"
                    , lineCounter
                    , DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                    , category
                    , AppDomainName
                    , Thread.CurrentThread.ManagedThreadId
                    , op.Id
                    )
                : string.Format("{0}\t{1}\t{2}\tA:{3}\tT:{4}\t\t{5}\t\t"
                    , lineCounter
                    , DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                    , category
                    , AppDomainName
                    , Thread.CurrentThread.ManagedThreadId
                    , isError ? "ERROR" : "");

            // smart formatting
            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (arg is string)
                        continue;
                    if (arg is System.Collections.IDictionary)
                        continue;
                    if (arg == null)
                    {
                        args[i] = "[null]";
                        continue;
                    }

                    var enumerable = arg as System.Collections.IEnumerable;
                    if (enumerable == null)
                        continue;

                    var sb = new StringBuilder("[");
                    foreach (var item in enumerable)
                    {
                        if (sb.Length > 1)
                            sb.Append(", ");
                        sb.Append(item);
                    }
                    sb.Append("]");
                    args[i] = sb.ToString();
                }
            }

            string msg;
            try
            {
                msg = Escape(string.Format(message, args ?? new object[0]));
            }
            catch (Exception e)
            {
                msg = Escape($"SNTRACE ERROR: {message}. {e.Message}");

                var thisModule = new StackTrace().GetFrame(0).GetMethod().Module;
                MethodBase callerMethod;
                var stackTrace = new StackTrace();

                for(var i = 2; ;i++)
                {
                    callerMethod = stackTrace.GetFrame(i).GetMethod();
                    if (thisModule != callerMethod.Module)
                        break;
                }

                msg += $". Caller: {callerMethod}, " + 
                    $"(type: {callerMethod.DeclaringType?.FullName ?? "?"}, " + 
                    $"asm: {callerMethod.Module.Name})";
            }

            line += msg;

            if (op != null)
                op.Message = msg;

            return line;
        }

        private static string Escape(string input)
        {
            var c = input.ToCharArray();
            for (var i = 0; i < c.Length; i++)
                if (c[i] < ' ' && c[i] != '\t')
                    c[i] = '.';
            return new string(c);
        }

        private static string FinishOperation(Operation op)
        {
            var lineCounter = Interlocked.Increment(ref _lineCounter);

            var line = string.Format("{0}\t{1}\t{2}\tA:{3}\tT:{4}\tOp:{5}\t{6}\t{7}\t{8}"
                , lineCounter
                , DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                , op.Category
                , AppDomainName
                , Thread.CurrentThread.ManagedThreadId
                , op.Id
                , op.Successful ? "End" : "UNTERMINATED"
                , (DateTime.UtcNow - op.StartedAt).ToString("hh':'mm':'ss'.'ffffff")
                , op.Message);

            return line;
        }

        /*================================================================== File writer */

        private static int _lineCounter;
        private static int _lastLineCounter;

        private static readonly Timer _timer = new Timer(_ => WriteToFile(), null, Config.WriteToFileDelay, Config.WriteToFileDelay);

        private static readonly object WriteSync = new object();
        private static void WriteToFile()
        {
            lock (WriteSync)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite); //stops the timer

                var text = CollectLines();
                if (text != null)
                {
                    using (var writer = new StreamWriter(LogFilePath, true))
                    {
                        writer.Write(text);
                        var lineCounter = _lineCounter;
                        if (lineCounter - _lastLineCounter > Config.LinesPerTrace)
                        {
                            var msg = string.Format("MaxPdiff: {0}", _maxPdiff);
                            writer.WriteLine(msg);

                            _lastLineCounter = lineCounter;
                        }
                    }
                }
                _timer.Change(Config.WriteToFileDelay, Config.WriteToFileDelay); //restart
            }
        }
        private static StringBuilder CollectLines()
        {
            var p0 = _lastBufferPosition;
            var p1 = Interlocked.Read(ref _bufferPosition);

            if (p0 == p1)
                return null;

            var sb = new StringBuilder(">"); // the '>' sign means: block writing start.
            var pdiff = p1 - p0;
            if (pdiff > _maxPdiff)
                _maxPdiff = pdiff;
            if (pdiff > Config.BufferSize)
            {
                sb.AppendFormat("BUFFER OVERRUN ERROR: Buffer size is {0}, unwritten lines : {1}", Config.BufferSize, pdiff).AppendLine();
            }

            while (p0 < p1)
            {
                var p = p0 % Config.BufferSize;
                var line = _buffer[p];
                sb.AppendLine(line);
                p0++;
            }

            _lastBufferPosition = p1;

            return sb;
        }

        private static readonly object _sync = new object();
        private static short _lineCount;
        private static string _logFilePath;
        private static string LogFilePath
        {
            get
            {
                if (_logFilePath == null || _lineCount >= Config.MaxWritesInOneFile)
                {
                    lock (_sync)
                    {
                        if (_logFilePath == null || _lineCount >= Config.MaxWritesInOneFile)
                        {
                            var logFilePath = Path.Combine(LogDirectory, "detailedlog_" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "Z.log");
                            if (!File.Exists(logFilePath))
                                using (var fs = new FileStream(logFilePath, FileMode.Create))
                                using (var wr = new StreamWriter(fs))
                                    wr.WriteLine("----");
                            _lineCount = 0;
                            Trace.WriteLine("SenseNet.Diagnostic.SnTrace file:" + logFilePath);
                            _logFilePath = logFilePath;
                        }
                    }
                }
                _lineCount++;
                return _logFilePath;
            }
        }

        /// <summary>
        /// Extends the given directory with the partial path of the detailed log directory ("App_Data\DetailedLog")
        /// </summary>
        /// <param name="baseDirectoryPath">Directory that will contain the log directory</param>
        public static string GetRelativeLogDirectory(string baseDirectoryPath)
        {
            return Path.Combine(baseDirectoryPath, "App_Data\\DetailedLog");
        }

        private static string __logDirectory;
        private static string LogDirectory
        {
            get
            {
                if (__logDirectory != null)
                    return __logDirectory;

                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\DetailedLog");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
                __logDirectory = logDirectory;
                Trace.WriteLine($"SenseNet.Diagnostic.SnTrace directory: {logDirectory}");
                return __logDirectory;
            }
        }

        /// <summary>
        /// Writes any buffered data to the underlying device and empties the internal buffer.
        /// </summary>
        public static void Flush()
        {
            WriteToFile();
        }
    }
}