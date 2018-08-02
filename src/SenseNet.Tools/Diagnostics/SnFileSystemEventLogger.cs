using System;
using System.Diagnostics;
using System.IO;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Built-in, lightweight IEventLogger implementation that writes events into a file.
    /// </summary>
    public class SnFileSystemEventLogger : SnEventloggerBase
    {
        /// <summary>
        /// Gets the number of entries that can be written into a log file before opening a new one.
        /// </summary>
        public long MaxWritesPerFile { get; }

        private bool _directoryChecked;
        private string _logDirectory;
        private string LogDirectory
        {
            get
            {
                if (!_directoryChecked)
                {
                    if (!DirectoryExists(_logDirectory))
                        CreateDirectory(_logDirectory);
                    _directoryChecked = true;
                }
                return _logDirectory;
            }
        }

        private readonly object _sync = new object();
        private short _entryCount;
        private string _logFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnFileSystemEventLogger"/> class.
        /// </summary>
        /// <param name="logDirectory">Directory for storing the log files.</param>
        /// <param name="maxWritesPerFile">Number of entries that can be written into a log file.</param>
        public SnFileSystemEventLogger(string logDirectory = null, int maxWritesPerFile = 100)
        {
            _logDirectory = logDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\EventLog");
            MaxWritesPerFile = maxWritesPerFile < 1 ? 10 : maxWritesPerFile;
        }

        /// <inheritdoc />
        /// <summary>
        /// Writes a message to the log file.
        /// </summary>
        protected override void WriteEntry(string entry, EventLogEntryType entryType, int eventId)
        {
            lock (_sync)
            {
                if (_logFilePath == null || _entryCount >= MaxWritesPerFile)
                {
                    var logFilePath = Path.Combine(LogDirectory,
                        "eventlog_" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "Z.log");
                    if (!FileExists(logFilePath))
                        CreateFile(logFilePath);
                    _entryCount = 0;
                    Trace.WriteLine("SenseNet.Diagnostic.SnLog file:" + logFilePath);
                    _logFilePath = logFilePath;
                }
                _entryCount++;

                WriteToFile(entry, _logFilePath);
            }
        }

        /// <summary>
        /// Determines whether the specified directory exists.
        /// </summary>
        protected virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
        /// <summary>
        /// Creates a directory.
        /// </summary>
        protected virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }
        /// <summary>
        /// Creates a log file.
        /// </summary>
        protected virtual void CreateFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var wr = new StreamWriter(fs))
                wr.WriteLine("----");
        }

        /// <summary>
        /// Writes a log entry to the specified file.
        /// </summary>
        protected virtual void WriteToFile(string entry, string fileName)
        {
            using (var writer = new StreamWriter(fileName, true))
                writer.Write(entry);
        }
    }
}
