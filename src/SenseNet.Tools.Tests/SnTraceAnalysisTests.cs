using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class SnTraceAnalysisTests : SnTraceTestClass
    {
        #region Logs for SessionReader test
        string[] _log1ForSessionReaderTest = new[]
        {
            "10\t2017-11-13 03:55:40.00000\tTest\tA:AppA\tT:42\t\t\t\tMsg400",
            "11\t2017-11-13 03:55:42.00000\tTest\tA:AppA\tT:42\t\t\t\tMsg420",
            "12\t2017-11-13 03:55:44.00000\tTest\tA:AppA\tT:42\t\t\t\tMsg440",
        };
        string[] _log2ForSessionReaderTest = new[]
        {
            "10\t2017-11-13 03:55:41.00000\tTest\tA:AppB\tT:42\t\t\t\tMsg410",
            "11\t2017-11-13 03:55:43.00000\tTest\tA:AppB\tT:42\t\t\t\tMsg430",
            "12\t2017-11-13 03:55:45.00000\tTest\tA:AppB\tT:42\t\t\t\tMsg450",
        };
        string[] _log3ForSessionReaderTest = new[]
        {
            "10\t2017-11-13 03:55:40.50000\tTest\tA:AppC\tT:42\t\t\t\tMsg405",
            "11\t2017-11-13 03:55:41.50000\tTest\tA:AppC\tT:42\t\t\t\tMsg415",
            "12\t2017-11-13 03:55:41.70000\tTest\tA:AppC\tT:42\t\t\t\tMsg417",
            "13\t2017-11-13 03:55:42.20000\tTest\tA:AppC\tT:42\t\t\t\tMsg422",
            "14\t2017-11-13 03:55:42.70000\tTest\tA:AppC\tT:42\t\t\t\tMsg427",
            "15\t2017-11-13 03:55:43.50000\tTest\tA:AppC\tT:42\t\t\t\tMsg435",
            "16\t2017-11-13 03:55:44.50000\tTest\tA:AppC\tT:42\t\t\t\tMsg445",
        };

        #endregion
        [TestMethod]
        public void SnTrace_Analysis_SessionReader()
        {
            var logs = new[] { _log1ForSessionReaderTest, _log2ForSessionReaderTest, _log3ForSessionReaderTest };

            // action
            string actual;
            using (var logFlow = Reader.Create(logs))
                actual = string.Join(",", logFlow.Select(e => e.Message));

            //  assert
            var expected = "Msg400,Msg405,Msg410,Msg415,Msg417,Msg420,Msg422,Msg427,Msg430,Msg435,Msg440,Msg445,Msg450";
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void SnTrace_Analysis_Filter()
        {
            // arrange
            var trace = new List<string>();
            using (UseTestWriter(trace))
            {
                SnTrace.EnableAll();
                WriteStructure1();
                SnTrace.DisableAll();
            }

            using (var logFlow = Reader.Create(trace))
            {
                // action
                var transformedLogFlow = logFlow
                    .Where(e => e.Status == Status.End)
                    .Where(e => e.Category == Category.Test);

                //  assert
                var actual = string.Join(", ", transformedLogFlow.Select(e => e.Message));
                Assert.AreEqual("10, 7, 14", actual);
            }
        }
        private void WriteStructure1()
        {
            SnTrace.Test.Write("1");
            SnTrace.Write("2");
            SnTrace.Test.Write("3");
            SnTrace.Write("4");
            SnTrace.Web.Write("5");
            SnTrace.Repository.Write("6");
            using (var op = SnTrace.Test.StartOperation("7"))
            {
                using (var op1 = SnTrace.StartOperation("8"))
                {
                    op1.Successful = true;
                }
                using (var op2 = SnTrace.Repository.StartOperation("9"))
                {
                    op2.Successful = true;
                }
                using (var op2 = SnTrace.Test.StartOperation("10"))
                {
                    op2.Successful = true;
                }
                using (var op2 = SnTrace.Test.StartOperation("11"))
                {
                    op2.Successful = false;
                }
                op.Successful = true;
            }
            SnTrace.Test.Write("12");
            SnTrace.Repository.Write("13");
            using (var op2 = SnTrace.Test.StartOperation("14"))
            {
                op2.Successful = true;
            }
            SnTrace.Repository.Write("15");
            SnTrace.Test.Write("16");

            SnTrace.Flush();
        }
    }
}
