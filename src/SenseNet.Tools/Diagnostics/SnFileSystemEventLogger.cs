using System;
using System.Diagnostics;
using System.IO;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    public class SnFileSystemEventLogger : SnEventloggerBase
    {
        public long MaxWritesPerFile { get; }

        private string _logDirectory;
        private string LogDirectory
        {
            get
            {
                if (_logDirectory == null)
                {
                    var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\EventLog");
                    if (!DirectoryExists(logDirectory))
                        CreateDirectory(logDirectory);
                    _logDirectory = logDirectory;
                }
                return _logDirectory;
            }
        }

        private readonly object _sync = new object();
        private short _entryCount;
        private string _logFilePath;
        private string LogFilePath
        {
            get
            {
                if (_logFilePath == null || _entryCount >= MaxWritesPerFile)
                {
                    lock (_sync)
                    {
                        if (_logFilePath == null || _entryCount >= MaxWritesPerFile)
                        {
                            var logFilePath = Path.Combine(LogDirectory, "eventlog_" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "Z.log");
                            if (!FileExists(logFilePath))
                                CreateFile(logFilePath);
                            _entryCount = 0;
                            Trace.WriteLine("SenseNet.Diagnostic.SnLog file:" + logFilePath);
                            _logFilePath = logFilePath;
                        }
                    }
                }
                _entryCount++;
                return _logFilePath;
            }
        }

        public SnFileSystemEventLogger(string logDirectory = null, int maxWritesPerFile = 100)
        {
            _logDirectory = logDirectory;
            MaxWritesPerFile = maxWritesPerFile < 1 ? 10 : maxWritesPerFile;
        }

        protected override void WriteEntry(string entry, EventLogEntryType entryType, int eventId)
        {
            //_entryCount++;
            WriteToFile(entry, LogFilePath);
        }

        protected virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
        protected virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }
        protected virtual void CreateFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var wr = new StreamWriter(fs))
                wr.WriteLine("----");
        }

        protected virtual void WriteToFile(string entry, string fileName)
        {
            using (var writer = new StreamWriter(fileName, true))
                writer.Write(entry);
        }
    }
}
