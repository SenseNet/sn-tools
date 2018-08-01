using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using SenseNet.Tools.Diagnostics;
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class SnLogTests
    {
        #region Nested classes
        private class TestEventLogger : TraceEventLogger
        {
            private readonly StringBuilder _stringBuilder = new StringBuilder();
            public string Written => _stringBuilder.ToString();

            protected override void Write(string msg)
            {
                _stringBuilder.AppendLine(msg);
            }
        }

        private class TestTracer : ISnTracer
        {
            public List<string> Lines { get; } = new List<string>();

            public void Write(string line)
            {
                Lines.Add(line);
            }

            public void Flush() { /* do nothing */ }
        }

        private class TestException_Warning : Exception, IEventTypeProvider
        {
            public TraceEventType EventType => TraceEventType.Warning;
            public TestException_Warning() { }
            public TestException_Warning(string message) : base(message) { }
            public TestException_Warning(string message, Exception innerException) : base(message, innerException) { }
            protected TestException_Warning(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        private class TestException_Information : Exception, IEventTypeProvider
        {
            public TraceEventType EventType => TraceEventType.Information;
            public TestException_Information() { }
            public TestException_Information(string message) : base(message) { }
            public TestException_Information(string message, Exception innerException) : base(message, innerException) { }
            protected TestException_Information(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        private class Class1ForLoggingReflectionTypeLoadEx { }
        private class Class2ForLoggingReflectionTypeLoadEx { }

        private class TestEventPropertyCollector : IEventPropertyCollector
        {
            private readonly IDictionary<string, object> _additionalProperties;

            public TestEventPropertyCollector(IDictionary<string, object> additionalProperties = null)
            {
                _additionalProperties = additionalProperties;
            }
            public IDictionary<string, object> Collect(IDictionary<string, object> properties)
            {
                if (_additionalProperties != null)
                    foreach (var key in _additionalProperties.Keys)
                        properties[key] = _additionalProperties[key];
                return properties;
            }
        }
        private class BuggyEventPropertyCollector : IEventPropertyCollector
        {
            private readonly string _errorMessage;
            public BuggyEventPropertyCollector(string errorMessage)
            {
                _errorMessage = errorMessage;
            }
            public IDictionary<string, object> Collect(IDictionary<string, object> properties)
            {
                throw new Exception(_errorMessage);
            }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private class TestEventEntry
        {
            public string Message { get; set; }
            public TraceEventType EventType { get; set; }
            public string Properties { get; set; }

            public override string ToString()
            {
                return $"{EventType}: {Message} ({Properties})";
            }
        }
        private class TestEventEntryLogger : IEventLogger
        {
            public readonly List<TestEventEntry> LogEntries = new List<TestEventEntry>();

            public void Write(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title,
                IDictionary<string, object> properties)
            {
                LogEntries.Add(new TestEventEntry
                {
                    Message = message.ToString(),
                    EventType = severity,
                    Properties = properties == null
                        ? string.Empty
                        : string.Join(", ", properties.Select(p => string.Concat(p.Key, ":", p.Value)))
                });
            }
        }
        private class TestAuditEvent : IAuditEvent
        {
            public int EventId { get; }
            public string Message { get; }
            public string Title { get; }
            public TestAuditEvent(string message, string title = null, int eventId = 42)
            {
                EventId = eventId;
                Message = message;
                Title = title ?? "TestAuditEvent";
            }
        }

        private class TestSnFileSystemEventLogger : SnFileSystemEventLogger
        {
            public Dictionary<string, List<string>> VirtualDirectory { get; } = new Dictionary<string, List<string>>();

            public TestSnFileSystemEventLogger(string logDirectory = null, int maxWritesPerFile = 100)
                : base(logDirectory, maxWritesPerFile) { }

            protected override void WriteToFile(string entry, string fileName)
            {
                VirtualDirectory[fileName].Add(entry);
            }

            protected override bool DirectoryExists(string path)
            {
                return true;
            }
            protected override void CreateDirectory(string path)
            {
                // do nothing
            }
            protected override bool FileExists(string path)
            {
                return VirtualDirectory.ContainsKey(path);
            }
            protected override void CreateFile(string path)
            {
                VirtualDirectory[path] = new List<string>();
            }
        }

        #endregion

        [TestMethod]
        public void SnLog_WriteException()
        {
            var loggerBackup = SnLog.Instance;
            var logger = new TestEventLogger();
            SnLog.Instance = logger;
            Exception thrown = null;

            try
            {
                var x = 1;
                var y = 2;
                Trace.WriteLine(y/(x - 1));
            }
            catch (Exception e)
            {
                thrown = e;
                SnLog.WriteException(e, eventId: 123456);
            }
            finally
            {
                SnLog.Instance = loggerBackup;
            }
            var written = logger.Written;
            Assert.IsNotNull(thrown);
            Assert.IsTrue(written.Contains("123456"));
            Assert.IsTrue(written.Contains(thrown.Message));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void SnLog_WriteTypeLoadException()
        {
            var loggerBackup = SnLog.Instance;
            var logger = new TestEventLogger();
            SnLog.Instance = logger;
            Exception thrown;

            try
            {
                var types = new[]
                {
                    typeof(Class1ForLoggingReflectionTypeLoadEx),
                    typeof(Class2ForLoggingReflectionTypeLoadEx)
                };
                var exceptions = new[]
                {
                    new Exception("Exception1"),
                    new Exception("Exception2"),
                };

                throw new ReflectionTypeLoadException(types, exceptions);
            }
            catch (Exception e)
            {
                thrown = e;
                SnLog.WriteException(e, eventId: 123456);
            }
            finally
            {
                SnLog.Instance = loggerBackup;
            }
            Assert.IsNotNull(thrown);

            var written = logger.Written.Split('\r', '\n')[0];

            Assert.IsTrue(written.Contains("123456"));
            Assert.IsTrue(written.Contains("Exception1"));
            Assert.IsTrue(written.Contains("Exception2"));
            Assert.IsTrue(written.Contains(typeof(Class1ForLoggingReflectionTypeLoadEx).FullName));
            Assert.IsTrue(written.Contains(typeof(Class2ForLoggingReflectionTypeLoadEx).FullName));
        }

        [TestMethod]
        public void SnLog_EventTypeProvider_Information()
        {
            var e = new Exception("E0");
            e = new TestException_Warning("E1", e);
            e = new Exception("E2", e);
            e = new TestException_Information("E3", e);
            e = new Exception("E4", e);

            //"GetEventType", e
            var snLogAcc = new PrivateType(typeof(SnLog));
            var actual = (TraceEventType)snLogAcc.InvokeStatic("GetEventType", e);

            Assert.AreEqual(TraceEventType.Information, actual);
        }

        [TestMethod]
        public void SnLog_EventTypeProvider_Warning()
        {
            var e = new Exception("E0");
            e = new TestException_Information("E1", e);
            e = new Exception("E2", e);
            e = new TestException_Warning("E3", e);
            e = new Exception("E4", e);

            //"GetEventType", e
            var snLogAcc = new PrivateType(typeof(SnLog));
            var actual = (TraceEventType)snLogAcc.InvokeStatic("GetEventType", e);

            Assert.AreEqual(TraceEventType.Warning, actual);
        }


        [TestMethod]
        public void SnLog_Write_DefaultPropertyCollector()
        {
            var loggerBackup = SnLog.Instance;
            var logger = new TestEventLogger();
            SnLog.Instance = logger;

            try
            {
                SnLog.WriteInformation("Informative message.", properties: new Dictionary<string, object>
                {
                    {"name1", "value1"},
                    {"name42", 42},
                });
            }
            finally
            {
                SnLog.Instance = loggerBackup;
            }

            var written = logger.Written;
            Assert.IsTrue(written.Contains("Informative message."));
            Assert.IsTrue(written.Contains("Properties: name1:value1, name42:42"));
        }
        [TestMethod]
        public void SnLog_Write_TestPropertyCollector()
        {
            var loggerBackup = SnLog.Instance;
            var logger = new TestEventLogger();
            SnLog.Instance = logger;

            var propertyCollectorBackup = SnLog.PropertyCollector;
            SnLog.PropertyCollector = new TestEventPropertyCollector(new Dictionary<string, object> { { "x", "y" } });

            try
            {
                SnLog.WriteInformation("Informative message.", properties: new Dictionary<string, object>
                {
                    {"name1", "value1"},
                    {"name42", 42},
                });
            }
            finally
            {
                SnLog.Instance = loggerBackup;
                SnLog.PropertyCollector = propertyCollectorBackup;
            }

            var written = logger.Written;
            Assert.IsTrue(written.Contains("Informative message."));
            Assert.IsTrue(written.Contains("Properties: name1:value1, name42:42, x:y"));
        }
        [TestMethod]
        public void SnLog_Write_BuggyPropertyCollector_FaultTolerant()
        {
            var loggerBackup = SnLog.Instance;
            var logger = new TestEventEntryLogger();
            SnLog.Instance = logger;

            var propertyCollectorBackup = SnLog.PropertyCollector;
            var errorMessage = "After all, the bug is just an animal.";
            SnLog.PropertyCollector = new BuggyEventPropertyCollector(errorMessage);

            try
            {
                SnLog.WriteInformation("Msg1",
                    properties: new Dictionary<string, object> {{"name1", "value1"}});
                SnLog.WriteInformation("Msg2",
                    properties: new Dictionary<string, object> {{"name42", 42}});
            }
            finally
            {
                SnLog.Instance = loggerBackup;
                SnLog.PropertyCollector = propertyCollectorBackup;
            }

            var entries = logger.LogEntries.Where(e=>e.EventType == TraceEventType.Information).ToArray();
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("Information: Msg1 (name1:value1)", entries[0].ToString());
            Assert.AreEqual("Information: Msg2 (name42:42)", entries[1].ToString());
        }

        [TestMethod]
        public void SnLog_Write_BuggyPropertyCollector_WritesErrorOnce()
        {
            var commonProperties = new Dictionary<string, object> {{"a", "b"}};

            var loggerBackup = SnLog.Instance;
            var logger = new TestEventEntryLogger();
            SnLog.Instance = logger;

            var propertyCollectorBackup = SnLog.PropertyCollector;
            var errorMessage1 = "After all, the bug is just an animal.";
            var errorMessage2 = "We can handle all the problems.";

            try
            {
                SnLog.WriteInformation("Msg1", properties: commonProperties);
                SnLog.WriteInformation("Msg2", properties: commonProperties);
                SnLog.PropertyCollector = new BuggyEventPropertyCollector(errorMessage1);
                SnLog.WriteInformation("Msg3", properties: commonProperties);
                SnLog.WriteInformation("Msg4", properties: commonProperties);
                SnLog.PropertyCollector = new BuggyEventPropertyCollector(errorMessage2);
                SnLog.WriteInformation("Msg5", properties: commonProperties);
                SnLog.WriteInformation("Msg6", properties: commonProperties);
            }
            finally
            {
                SnLog.Instance = loggerBackup;
                SnLog.PropertyCollector = propertyCollectorBackup;
            }

            var entries = logger.LogEntries.Select(e=>e.ToString()).ToArray();
            Assert.AreEqual(8, entries.Length);
            Assert.AreEqual("Information: Msg1 (a:b)", entries[0]);
            Assert.AreEqual("Information: Msg2 (a:b)", entries[1]);
            Assert.AreEqual($"Error: {errorMessage1} ", entries[2]
                .Substring(0, entries[2].IndexOf("(", StringComparison.Ordinal)));
            Assert.AreEqual("Information: Msg3 (a:b)", entries[3]);
            Assert.AreEqual("Information: Msg4 (a:b)", entries[4]);
            Assert.AreEqual($"Error: {errorMessage2} ", entries[5]
                .Substring(0, entries[5].IndexOf("(", StringComparison.Ordinal)));
            Assert.AreEqual("Information: Msg5 (a:b)", entries[6]);
            Assert.AreEqual("Information: Msg6 (a:b)", entries[7]);
        }

        [TestMethod]
        public void SnLog_Write_AllWriteMethodCollectProperties()
        {
            var commonProperties = new Dictionary<string, object> { { "a", "b" } };

            var loggerBackup = SnLog.Instance;
            var logger = new TestEventEntryLogger();
            SnLog.Instance = logger;

            var propertyCollectorBackup = SnLog.PropertyCollector;
            SnLog.PropertyCollector = new TestEventPropertyCollector(new Dictionary<string, object> { { "x", "y" } });

            try
            {
                SnLog.WriteInformation("Msg1", properties: commonProperties);
                SnLog.WriteWarning("Msg2", properties: commonProperties);
                SnLog.WriteAudit(new TestAuditEvent("Msg3"), properties: commonProperties);
                SnLog.WriteError("Msg4", properties: commonProperties);
                SnLog.WriteException(new Exception("Msg5"), properties: commonProperties);
            }
            finally
            {
                SnLog.Instance = loggerBackup;
                SnLog.PropertyCollector = propertyCollectorBackup;
            }

            var entries = logger.LogEntries.Select(e => e.ToString()).ToArray();
            Assert.AreEqual(5, entries.Length);
            Assert.AreEqual("Information: Msg1 (a:b, x:y)", entries[0]);
            Assert.AreEqual("Warning: Msg2 (a:b, x:y)", entries[1]);
            Assert.AreEqual("Verbose: Msg3 (a:b, x:y)", entries[2]);
            Assert.AreEqual("Error: Msg4 (a:b, x:y)", entries[3]);
            Assert.AreEqual("Error: Msg5 (a:b, x:y,", entries[4].Substring(0, 8 + entries[2].IndexOf("(", StringComparison.Ordinal)));
        }

        [TestMethod]
        public void SnLog_Write_BindToSnTrace()
        {
            var commonProperties = new Func<Dictionary<string, object>>(() => new Dictionary<string, object> {{"a", "b"}});

            var loggerBackup = SnLog.Instance;
            var logger = new TestEventEntryLogger();
            SnLog.Instance = logger;

            var propertyCollectorBackup = SnLog.PropertyCollector;
            SnLog.PropertyCollector = new TestEventPropertyCollector(new Dictionary<string, object> { { "x", "y" } });

            var tracersBackup = SnTrace.SnTracers.ToArray();
            SnTrace.SnTracers.Clear();
            var tracer = new TestTracer();
            SnTrace.SnTracers.Add(tracer);

            var eventEnabledBackup = SnTrace.Event.Enabled;
            SnTrace.Event.Enabled = true;

            // action 
            try
            {
                SnLog.WriteInformation("Msg1", properties: commonProperties());
                SnLog.WriteWarning("Msg2", properties: commonProperties());
                SnLog.WriteAudit(new TestAuditEvent("Msg3"), properties: commonProperties());
                SnLog.WriteError("Msg4", properties: commonProperties());
                SnLog.WriteException(new Exception("Msg5"), properties: commonProperties());
            }
            finally
            {
                SnLog.Instance = loggerBackup;
                SnLog.PropertyCollector = propertyCollectorBackup;

                SnTrace.SnTracers.Clear();
                SnTrace.SnTracers.AddRange(tracersBackup);

                SnTrace.Event.Enabled = eventEnabledBackup;
            }

            var entries = logger.LogEntries; //.Select(e => e.ToString()).ToArray();
            var traceLines = tracer.Lines;

            Assert.AreEqual(5, entries.Count);
            Assert.AreEqual(5, traceLines.Count);

            for (int i = 0; i < entries.Count; i++)
                CheckBinding(entries[i], traceLines[i], i);
            // Information: Msg1 (a:b, x:y, SnTrace:#b18b0463-45c1-4e72-8882-8837df131556)
            // 1	2018-08-01 00:15:33.85820	Event	A:UnitTestAdapter: Running test	T:9				INFORMATION #b18b0463-45c1-4e72-8882-8837df131556: Msg1
        }
        private void CheckBinding(TestEventEntry logEntry, string traceLine, int testCase)
        {
            var log = GetGuidAndMessageFromLog(logEntry, testCase);
            var trace = GetGuidAndMessageFromTrace(traceLine);
            Assert.AreEqual(log, trace);
        }
        private string GetGuidAndMessageFromLog(TestEventEntry logEntry, int testCase)
        {
            // a:b, x:y, SnTrace:#b18b0463-45c1-4e72-8882-8837df131556
            var bindingInfo = logEntry.Properties.Split(',')
                .Select(x => x.Trim())
                .FirstOrDefault(x => x.StartsWith("SnTrace:#"));
            Assert.IsNotNull(bindingInfo, "Binding info not found in test case " + testCase);

            var guid = bindingInfo.Substring("SnTrace:#".Length);
            if(logEntry.EventType == TraceEventType.Verbose)
                return $"{guid}|{logEntry.Message}";
            return $"{logEntry.EventType.ToString().ToUpperInvariant()}|{guid}|{logEntry.Message}";
        }
        private string GetGuidAndMessageFromTrace(string line)
        {
            // 1	2018-08-01 00:15:33.85820	Event	A:UnitTestAdapter: Running test	T:9				INFORMATION #b18b0463-45c1-4e72-8882-8837df131556: Msg1
            var bindingInfo = line.Split('\t').Last();

            // INFORMATION #b18b0463-45c1-4e72-8882-8837df131556: Msg1
            // AUDIT #3b3f725e-5f4e-4ce4-89a9-fd780c70a7d9: Msg3, Id:[null], Path:[null]
            return bindingInfo.Replace("AUDIT ", "").Replace(", Id:-, Path:-", "").Replace("#", "").Replace(":", "").Replace(" ", "|");
        }

        [TestMethod]
        public void SnLog_WriteAndReload_SnEventLogger()
        {
            var testValue = Guid.NewGuid().ToString();

            // action
            new SnEventLogger("SenseNet", "SenseNetInstrumentation")
                .Write(testValue, null, 0, 0, TraceEventType.Information, null,
                    new Dictionary<string, object> { { "a", "b" }, { "x", "y" } });

            // assert
            var logs = EventLog.GetEventLogs();
            var log = logs.FirstOrDefault(l => l.LogDisplayName == "SenseNet");
            Assert.IsNotNull(log);
            var entries = new List<EventLogEntry>();
            foreach (EventLogEntry entry in log.Entries)
                entries.Add(entry);
            var lastEntry = entries.Last();

            var entryData = ParseEventlogEntryData(lastEntry.Message);

            Assert.AreEqual(testValue, entryData["Message"]);
            Assert.AreEqual("Information", entryData["Severity"]);
            Assert.AreEqual(Environment.MachineName, entryData["Machine"]);
            Assert.AreEqual("a - b, x - y", entryData["Extended Properties"]);
        }
        private Dictionary<string, string> ParseEventlogEntryData(string text)
        {
            var result = new Dictionary<string, string>();
            var fields = text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var index = 0;
            while (true)
            {
                var field = fields[index++];
                var p = field.IndexOf(':');
                var name = field.Substring(0, p);
                var value = field.Length > p ? field.Substring(p + 1).Trim() : string.Empty;
                if (name != "Extended Properties")
                {
                    result.Add(name, value);
                    continue;
                }
                var extendedValue = new StringBuilder(value);
                for (int i = index; i < fields.Length; i++)
                    extendedValue.Append(", ").Append(fields[i]);
                result.Add(name, extendedValue.ToString());
                break;
            }
            return result;
        }

        [TestMethod]
        public void SnLog_WriteAndReload_SnSnFileSystemEventLogger()
        {
            var testValue = Guid.NewGuid().ToString();
            var logger = new TestSnFileSystemEventLogger(@"X:\MyLog");

            // action
            logger.Write(testValue, null, 0, 0, TraceEventType.Information, null,
                new Dictionary<string, object> { { "a", "b" }, { "x", "y" } });

            // assert
            var logs = logger.VirtualDirectory;
            Assert.AreEqual(1, logs.Count);

            var lastEntry = logs.First().Value.Last();

            var entryData = ParseEventlogEntryData(lastEntry);

            Assert.AreEqual(testValue, entryData["Message"]);
            Assert.AreEqual("Information", entryData["Severity"]);
            Assert.AreEqual(Environment.MachineName, entryData["Machine"]);
            Assert.AreEqual("a - b, x - y", entryData["Extended Properties"]);
        }
        [TestMethod]
        public void SnLog_WriteAndReload_SnSnFileSystemEventLogger_DefaultDirectory()
        {
            var testValue = Guid.NewGuid().ToString();
            var logger = new TestSnFileSystemEventLogger();

            // action
            logger.Write(testValue, null, 0, 0, TraceEventType.Information, null, null);

            // assert
            var logs = logger.VirtualDirectory;
            Assert.AreEqual(1, logs.Count);

            var fileName = logs.First().Key;
            var dirName = (Path.GetDirectoryName(fileName) ?? string.Empty).ToLowerInvariant();
            var expected = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data\\EventLog").ToLowerInvariant();
            Assert.AreEqual(expected, dirName);
        }
        [TestMethod]
        public void SnLog_WriteAndReload_SnSnFileSystemEventLogger_Files()
        {
            var logger = new TestSnFileSystemEventLogger(@"X:\MyLog", 2);

            // action
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(600);
                logger.Write("Msg" + i, null, 0, i, TraceEventType.Information, null, null);
            }

            // assert
            var logs = logger.VirtualDirectory;
            Assert.AreEqual(3, logs.Count);

            var allEntries = logs
                .SelectMany(x => x.Value)
                .Select(ParseEventlogEntryData)
                .OrderBy(e => e["EventId"])
                .Select(e=>$"{e["Message"]}:{e["EventId"]}")
                .ToArray();
            var entryData = string.Join(", ", allEntries);

            Assert.AreEqual("Msg0:0, Msg1:1, Msg2:2, Msg3:3, Msg4:4", entryData);
        }

    }
}
