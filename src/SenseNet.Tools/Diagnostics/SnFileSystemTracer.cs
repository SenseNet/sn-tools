﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
// ReSharper disable StringLiteralTypo

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// An ISnTracer implementation that persists the SnTrace output to the file system. 
    /// </summary>
    public class SnFileSystemTracer : BufferedSnTracer
    {
        internal class Config
        {
            public readonly long BufferSize;
            public readonly int WriteToFileDelay;
            public readonly short MaxWritesInOneFile;

            private const string ConfigSection = "sensenet/detailedLogger";

            internal Config()
            {
                var collection = ConfigurationManager.GetSection(ConfigSection) as NameValueCollection;
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

        private readonly Config _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnFileSystemTracer"/> class.
        /// </summary>
        public SnFileSystemTracer()
        {
            _config = new Config();
            Initialize(_config.BufferSize, _config.WriteToFileDelay);
        }

        /// <inheritdoc />
        /// <param name="text"></param>
        protected override void WriteBatch(string text)
        {
            IOException lastError = null;

            // Max 3 attempt.
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // Write the text.
                    using (var writer = new StreamWriter(LogFilePath, true))
                        writer.Write(text);
                    // Return if everything is OK.
                    return;
                }
                catch (IOException e)
                {
                    // Memorize the exception.
                    lastError = e;
                    // Open a new file immediately and try again.
                    _logFilePath = null;
                }
            }

            // Throw the last error.
            if (lastError != null)
                throw lastError;

            // General error message.
            throw new ApplicationException("Something went wrong" +
                " when we tried to write a file in the following directory: " + LogDirectory);
        }

        /* ===================================================================== */

        private readonly object _sync = new object();
        private short _lineCount;
        private string _logFilePath;
        private string LogFilePath
        {
            get
            {
                if (_logFilePath == null || _lineCount >= _config.MaxWritesInOneFile)
                {
                    lock (_sync)
                    {
                        if (_logFilePath == null || _lineCount >= _config.MaxWritesInOneFile)
                        {
                            var logFilePath = Path.Combine(LogDirectory, "detailedlog_" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-ffff") + "Z.log");
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
            return Path.Combine(baseDirectoryPath, "App_Data", "DetailedLog");
        }

        // ReSharper disable once InconsistentNaming
        private static string __logDirectory;
        private static string LogDirectory
        {
            get
            {
                if (__logDirectory != null)
                    return __logDirectory;

                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DetailedLog");
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);
                __logDirectory = logDirectory;
                Trace.WriteLine($"SenseNet.Diagnostic.SnTrace directory: {logDirectory}");
                return __logDirectory;
            }
        }
    }
}
