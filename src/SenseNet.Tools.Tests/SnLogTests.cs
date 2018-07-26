using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using SenseNet.Tools.Diagnostics;
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

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
    }
}
