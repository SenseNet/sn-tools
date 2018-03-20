using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class ExceptionTests
    {
        #region Nested classes

        internal class MyCustomException : SnException
        {
            internal static int MyCustomEventId = 1234567;

            public MyCustomException() : base(MyCustomEventId)
            {
            }
            public MyCustomException(string message) : base(MyCustomEventId, message)
            {
            }
            public MyCustomException(string message, Exception inner) : base(MyCustomEventId, message, inner)
            {
            }
            public MyCustomException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        #endregion

        [TestMethod]
        public void Exceptions_GetEventId_SnException()
        {
            var eventId = SnLog.GetEventId(new MyCustomException("MSG"));

            Assert.AreEqual(MyCustomException.MyCustomEventId, eventId);
        }
        [TestMethod]
        public void Exceptions_GetEventId_SnException_Deep()
        {
            // find event id in inner exception
            var deepException = new Exception("MSG1",
                new Exception("MSG2",
                    new MyCustomException("MSG3")));

            var eventId = SnLog.GetEventId(deepException);

            Assert.AreEqual(MyCustomException.MyCustomEventId, eventId);
        }
        [TestMethod]
        public void Exceptions_GetEventId_Data()
        {
            // find event id in custom data of a simple exception
            const int externalEventId = 12345;
            var deepException = new Exception("MSG1", 
                new Exception("MSG2", 
                    new Exception("MSG3")
                    {
                        Data = {{ "EventId", externalEventId }}
                    }));

            var eventId = SnLog.GetEventId(deepException);

            Assert.AreEqual(externalEventId, eventId);
        }

        [TestMethod]
        public void Exceptions_GetTypeLoadTypes()
        {
            TypeLoadException typeLoadEx = null;

            try
            {
                // cause a type load exception to have it filled with a type name
                Assembly.GetExecutingAssembly().GetType("NonExistentType.TypeLoadException", true);
            }
            catch (TypeLoadException ex)
            {
                typeLoadEx = ex;
            }

            var rtle = new ReflectionTypeLoadException(new Type[0], new Exception[]
            {
                new FileLoadException("error", "FileLoadException.dll"),
                new FileNotFoundException("error", "FileNotFoundException.dll"), 
                new BadImageFormatException("error", "BadImageFormatException.dll"), 
                new SecurityException("error") { Url = "SecurityException.dll"}, 
                typeLoadEx
            });

            var types = Utility.GetTypeLoadErrorTypes(rtle);

            Assert.AreEqual(string.Join(",", 
                    "FileLoadException.dll", 
                    "FileNotFoundException.dll", 
                    "BadImageFormatException.dll", 
                    "SecurityException.dll", 
                    "NonExistentType.TypeLoadException"),
                string.Join(",", types));
        }
    }
}
