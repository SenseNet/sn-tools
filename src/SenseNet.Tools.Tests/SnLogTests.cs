using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class SnLogTests
    {
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
            public TraceEventType EventType { get { return TraceEventType.Warning; } }
            public TestException_Warning() { }
            public TestException_Warning(string message) : base(message) { }
            public TestException_Warning(string message, Exception innerException) : base(message, innerException) { }
            protected TestException_Warning(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
        private class TestException_Information : Exception, IEventTypeProvider
        {
            public TraceEventType EventType { get { return TraceEventType.Information; } }
            public TestException_Information() { }
            public TestException_Information(string message) : base(message) { }
            public TestException_Information(string message, Exception innerException) : base(message, innerException) { }
            protected TestException_Information(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

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
    }
}
