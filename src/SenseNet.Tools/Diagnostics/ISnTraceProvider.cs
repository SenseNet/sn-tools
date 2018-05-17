using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

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

    public abstract class BufferedSnTraceProvider : ISnTraceProvider
    {
        private string[] _buffer;
        private long _bufferSize;
        private long _bufferPosition; // this field is incremented by every logger thread.
        private long _lastBufferPosition; // this field is written by only CollectLines method.
        private long _writeDelay;
        private long _blockSizeWarning;

        /// <summary>Statistical data: the longest gap between p0 and p1</summary>
        private long _maxPdiff;

        private Timer _timer;

        public virtual void Initialize(long bufferSize, int writeDelay)
        {
            _bufferSize = bufferSize;
            _blockSizeWarning = Math.Max(bufferSize / 5, 100);
            _buffer = new string[_bufferSize];
            _writeDelay = writeDelay;

            _timer = new Timer(_ => TimerTick(), null, _writeDelay, _writeDelay);
        }

        public virtual void Write(string line)
        {
            // writing to the buffer
            var p = Interlocked.Increment(ref _bufferPosition) - 1;
            _buffer[p % _bufferSize] = line;
        }

        private object _writeSync = new object();
        private void TimerTick()
        {
            lock (_writeSync)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite); //stops the timer

                var text = CollectLines();
                if (text != null)
                    WriteTo(text.ToString());

                _timer.Change(_writeDelay, _writeDelay); //restart
            }
        }
        private StringBuilder CollectLines()
        {
            var p0 = _lastBufferPosition;
            var p1 = Interlocked.Read(ref _bufferPosition);

            if (p0 == p1)
                return null;

            var sb = new StringBuilder(">"); // the '>' sign means: block writing start.
            var pdiff = p1 - p0;
            if (pdiff > _maxPdiff)
                _maxPdiff = pdiff;


            if (pdiff > _bufferSize)
                sb.AppendFormat("BUFFER OVERRUN ERROR: Buffer size is {0}, unwritten lines : {1}", _bufferSize, pdiff).AppendLine();

            while (p0 < p1)
            {
                var p = p0 % _bufferSize;
                var line = _buffer[p];
                sb.AppendLine(line);
                p0++;
            }

            _lastBufferPosition = p1;

            // If the block contains more than 20% of the buffer size, write a message
            if (pdiff > _blockSizeWarning)
                sb.AppendFormat("Block size reaches the risky limit: {0}. Buffer size: {1}", pdiff, _bufferSize).AppendLine();

            return sb;
        }

        public virtual void Flush()
        {
            TimerTick();
        }

        protected abstract void WriteTo(string message);
    }

    public class SnFileSystemTraceProvider : BufferedSnTraceProvider
    {
        internal class Config
        {
            public readonly long BufferSize;
            public readonly int WriteToFileDelay;
            public readonly short MaxWritesInOneFile;

            private readonly string[] AvailableSections = { "detailedLogger", "sensenet/detailedLogger" };

            internal Config()
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
            }

            internal static T Parse<T>(NameValueCollection collection, string key, T defaultValue)
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
                    throw new ApplicationException($"Invalid configuration: key: '{key}', value: '{value}'.", e);
                }
            }
        }

        private Config _config;

        /// <summary>Statistical data: the longest gap between p0 and p1</summary>
        private static long _maxPdiff;

        public SnFileSystemTraceProvider()
        {
            _config = new Config();
            base.Initialize(_config.BufferSize, _config.WriteToFileDelay);
        }

        protected override void WriteTo(string text)
        {
            using (var writer = new StreamWriter(LogFilePath, true))
                writer.Write(text);
        }

        /* ===================================================================== */

        private readonly object Sync = new object();
        private short _lineCount;
        private string _logFilePath;
        private string LogFilePath
        {
            get
            {
                if (_logFilePath == null || _lineCount >= _config.MaxWritesInOneFile)
                {
                    lock (Sync)
                    {
                        if (_logFilePath == null || _lineCount >= _config.MaxWritesInOneFile)
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

        // ReSharper disable once InconsistentNaming
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

    }
}
