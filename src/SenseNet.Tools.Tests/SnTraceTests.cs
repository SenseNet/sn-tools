using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using SenseNet.Diagnostics.Analysis;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class SnTraceTests : SnTraceTestClass
    {
        private void AssertOneErrorLine(IReadOnlyList<string> log, string expectedSubstring)
        {
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual(1, log.Count);
            Assert.IsTrue(msg.StartsWith("SNTRACE ERROR:"));
            Assert.IsTrue(msg.Contains(expectedSubstring));
        }

        //[TestMethod]
        //public void SnTrace_InfiniteWriting()
        //{
        //    // WARNING
        //    // This method causes an infinite loop. Designed for a manual test.
        //    // Before the fix this test threw an IOException with a similar message:
        //    // "The process cannot access the file '...' because it is being used by another process."
        //    //
        //    // The manual test steps:
        //    //   1 - PREPARATION
        //    //     - Uncomment the whole test method.
        //    //     - Start only this test.
        //    //     - Search the log file in the bin\Debug\App_Data\DetailedLog directory
        //    //       (example name: detailedlog_20200116-052425-7334Z.log)
        //    //   2 - TEST ACTION
        //    //     - Open this file a file-blocker application (e.g. Microsoft Word)
        //    //   3 - ASSERT
        //    //     - The test runs continuously and a new log file created.
        //    //   4 - COMPLETION
        //    //     - Stop the running test.
        //    //     - Comment out the whole test method.

        //    CleanupAndEnableAll();

        //    while (true)
        //    {
        //        SnTrace.Write("test-line");
        //        SnTrace.Flush();
        //        System.Threading.Thread.Sleep(1000);
        //    }
        //}

        [TestMethod]
        public void SnTrace_Write2lines()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf");
            SnTrace.Write("qwer");

            var log = DisableAllAndGetLog();

            Assert.AreEqual(2, log.Count);
            Assert.IsTrue(log[0].EndsWith("asdf"));
            Assert.IsTrue(log[1].EndsWith("qwer"));
        }

        [TestMethod]
        public void SnTrace_WriteError()
        {
            CleanupAndEnableAll();

            SnTrace.WriteError("asdf");

            var log = DisableAllAndGetLog();

            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("ERROR", GetColumnFromLine(log[0], 6));
            Assert.IsTrue(log[0].EndsWith("asdf"));
        }


        [TestMethod]
        public void SnTrace_WriteEmpty_WithoutParams()
        {
            CleanupAndEnableAll();

            SnTrace.Write(null);

            var log = DisableAllAndGetLog();
            AssertOneErrorLine(log, "Value cannot be null");
        }

        [TestMethod]
        public void SnTrace_WriteEmpty_WithParams()
        {
            CleanupAndEnableAll();

            SnTrace.Write(null, 1, "asdf");

            var log = DisableAllAndGetLog();
            AssertOneErrorLine(log, "Value cannot be null");
        }
        [TestMethod]
        public void SnTrace_WriteNotEmpty_NullParams()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf: {0}", null);

            var log = DisableAllAndGetLog();
            AssertOneErrorLine(log, "asdf: {0}");
        }
        [TestMethod]
        public void SnTrace_WriteNotEmpty_NullValue()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf: {0}, {1}, {2}", 42, null, "asdf");

            var log = DisableAllAndGetLog();
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual("asdf: 42, [null], asdf", msg);
        }


        [TestMethod]
        public void SnTrace_Operation_Nested()
        {
            CleanupAndEnableAll();
            ResetOperationId();

            using (var op1 = SnTrace.StartOperation("Op1"))
            {
                // ReSharper disable once UnusedVariable
                using (var op2 = SnTrace.StartOperation("Op2"))
                {
                    SnTrace.Write("asdf");
                }
                using (var op3 = SnTrace.StartOperation("Op3"))
                {
                    SnTrace.Write("qwer");
                    op3.Successful = false;
                }
                using (var op4 = SnTrace.StartOperation("Op4"))
                {
                    SnTrace.Write("yxcv");
                    op4.Successful = true;
                }
                op1.Successful = true;
            }

            var log = DisableAllAndGetLog();

            Assert.AreEqual(11, log.Count);

            var messages = string.Join(", ", log.Select(GetMessageFromLine));
            Assert.AreEqual("Op1, Op2, asdf, Op2, Op3, qwer, Op3, Op4, yxcv, Op4, Op1", messages);

            var operationData = string.Join(", ", log.Select(x => GetColumnFromLine(x, 5) + " " + GetColumnFromLine(x, 6)));
            Assert.AreEqual("Op:1 Start, Op:2 Start,  , Op:2 UNTERMINATED, Op:3 Start,  , Op:3 UNTERMINATED, Op:4 Start,  , Op:4 End, Op:1 End", operationData);
        }


        [TestMethod]
        public void SnTrace_SmartFormat_IntArray()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf: {0}", new[] { 1, 2, 3, 4 });

            var log = DisableAllAndGetLog();
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual("asdf: [1, 2, 3, 4]", msg);
        }
        [TestMethod]
        public void SnTrace_SmartFormat_StringList()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf: {0}", new List<string>(new[] { "asdf", "qwer", "yxcv" }));

            var log = DisableAllAndGetLog();
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual("asdf: [asdf, qwer, yxcv]", msg);
        }
        [TestMethod]
        public void SnTrace_SmartFormat_IntList()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf: {0}", new List<int>(new[] { 1, 2, 3, 4 }));

            var log = DisableAllAndGetLog();
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual("asdf: [1, 2, 3, 4]", msg);
        }


        [TestMethod]
        public void SnTrace_SmartFormat_LinqExpressionWhenEnabled()
        {
            CleanupAndEnableAll();
            _expressionExecuted = false;

            SnTrace.Write("asdf: {0}", Filter(Enumerable.Range(40, 5)));

            var log = DisableAllAndGetLog();
            var msg = GetMessageFromLine(log[0]);

            Assert.AreEqual(1, log.Count);
            Assert.IsTrue(_expressionExecuted);
            Assert.AreEqual("asdf: [41, 42, 43]", msg);
        }
        [TestMethod]
        public void SnTrace_SmartFormat_LinqExpressionWhenDisabled()
        {
            CleanupAndEnableAll();
            _expressionExecuted = false;
            SnTrace.Custom.Enabled = false;

            SnTrace.Write("asdf: {0}", Filter(Enumerable.Range(40, 5)));

            var log = DisableAllAndGetLog();

            Assert.AreEqual(0, log.Count);
            Assert.IsFalse(_expressionExecuted);
        }
        private bool _expressionExecuted;
        private IEnumerable<int> Filter(IEnumerable<int> input)
        {
            return input.Where(i => i > 40 && i < 44 && FilterMethod());
        }
        private bool FilterMethod()
        {
            _expressionExecuted = true;
            return true;
        }


        [TestMethod]
        public void SnTrace_CategoriesContainsAll()
        {
            var categoryFields = GetCategoryFields(out var names).ToArray();
            Assert.AreEqual("", string.Join(", ", SnTrace.Categories.Except(categoryFields).Select(x => x.Name)));
            Assert.AreEqual("", string.Join(", ", categoryFields.Except(SnTrace.Categories).Select(x => x.Name)));
            Assert.AreEqual("", string.Join(", ", names.Except(SnTrace.Categories.Select(x => x.Name))));
            Assert.AreEqual("", string.Join(", ", SnTrace.Categories.Select(x => x.Name).Except(names)));

            var categoryNames = SnTrace.Categories.Select(c=>c.Name).ToArray();
            var analysisCategoryNames = GetAnalysisCategoryFields(out names);
            Assert.AreEqual("", string.Join(", ", categoryNames.Except(analysisCategoryNames)));
            Assert.AreEqual("", string.Join(", ", analysisCategoryNames.Except(categoryNames)));
            Assert.AreEqual("", string.Join(", ", analysisCategoryNames.Except(names)));
            Assert.AreEqual("", string.Join(", ", names.Except(analysisCategoryNames)));
        }
        private static IEnumerable<SnTrace.SnTraceCategory> GetCategoryFields(out string[] catNames)
        {
            var type = typeof(SnTrace);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            var catFields = fields.Where(f => f.FieldType == typeof(SnTrace.SnTraceCategory)).ToArray();
            catNames = catFields.Select(f => f.Name).ToArray();
            var cats = catFields.Select(f => (SnTrace.SnTraceCategory)f.GetValue(null)).ToArray();
            return cats;
        }
        private static string[] GetAnalysisCategoryFields(out string[] catNames)
        {
            var type = typeof(Category);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            var catFields = fields.Where(f => f.FieldType == typeof(string)).ToArray();
            catNames = catFields.Select(f => f.Name).ToArray();
            var cats = catFields.Select(f => (string)f.GetValue(null)).ToArray();
            return cats;
        }


        [TestMethod]
        public void SnTrace_Entries_3lines()
        {
            CleanupAndEnableAll();

            SnTrace.Write("asdf asdf");
            SnTrace.Write("qwer\nqwer");
            SnTrace.Write("yxcv\tyxcv");

            var log = DisableAllAndGetLog();

            var entries = log.Select(Entry.Parse).Where(e => e != null).ToArray();

            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual("asdf asdf", entries[0].Message);
            Assert.AreEqual("qwer.qwer", entries[1].Message);
            Assert.AreEqual("yxcv\tyxcv", entries[2].Message);
        }
        [TestMethod]
        public void SnTrace_Entries_Operations()
        {
            CleanupAndEnableAll();

            using (var op1 = SnTrace.StartOperation("Op1"))
            {
                // ReSharper disable once UnusedVariable
                using (var op2 = SnTrace.StartOperation("Op2"))
                {
                    SnTrace.Write("asdf");
                }
                using (var op3 = SnTrace.StartOperation("Op3"))
                {
                    SnTrace.Write("qwer");
                    op3.Successful = false;
                }
                using (var op4 = SnTrace.StartOperation("Op4"))
                {
                    SnTrace.Write("yxcv");
                    op4.Successful = true;
                }
                op1.Successful = true;
            }

            var log = DisableAllAndGetLog();

            var entries = log.Select(Entry.Parse).Where(e=>e!= null).ToArray();

            Assert.AreEqual(11, entries.Length);

            var operationData = string.Join(",", entries.Select(x => x.Status+":"+x.Message));
            Assert.AreEqual("Start:Op1,Start:Op2,:asdf,UNTERMINATED:Op2,Start:Op3,:qwer,UNTERMINATED:Op3,Start:Op4,:yxcv,End:Op4,End:Op1", operationData);
            Assert.AreEqual(entries[0].OpId, entries[10].OpId);
            Assert.AreEqual(entries[1].OpId, entries[3].OpId);
            Assert.AreEqual(entries[4].OpId, entries[6].OpId);
            Assert.AreEqual(entries[7].OpId, entries[9].OpId);
        }

        [TestMethod]
        public void SnTrace_DynamicCategories()
        {
            // Test initialization
            CleanupAndEnableAll();

            // Activate Custom and Test categories. Any other be inactive.
            SnTrace.DisableAll();
            SnTrace.Custom.Enabled = true;
            SnTrace.Test.Enabled = true;

            // Write 7 lines including dynamic categories
            SnTrace.Write("Line1");
            SnTrace.Category("asdf").Write("Line2");
            SnTrace.Test.Write("Line3");
            SnTrace.Category("qwer").Write("Line4");
            SnTrace.Test.Write("Line5");
            SnTrace.Category("yxcv").Write("Line6");
            SnTrace.Write("Line7");

            // Get log
            var log = DisableAllAndGetLog();

            // Get categories
            var categories = log
                .Select(Entry.Parse)
                .Where(e => e != null)
                .Select(e => e.Category)
                .ToArray();
            var actual = string.Join(",", categories);

            // Verify
            Assert.AreEqual("Custom,asdf,Test,qwer,Test,yxcv,Custom", actual);
        }

        [TestMethod]
        public void SnTrace_DynamicCategoryOnOff()
        {
            // Test initialization
            CleanupAndEnableAll();

            // Activate Custom and Test categories. Any other be inactive.
            SnTrace.DisableAll();
            SnTrace.Custom.Enabled = true;
            SnTrace.Test.Enabled = true;

            // Pin custom categories
            var asdf = SnTrace.Category("asdf");
            var qwer = SnTrace.Category("qwer");
            var yxcv = SnTrace.Category("yxcv");

            asdf.Write("0");
            qwer.Write("1");
            yxcv.Write("2");
            asdf.Enabled = false;
            asdf.Write("3");
            qwer.Write("4");
            yxcv.Write("5");
            yxcv.Enabled = false;
            asdf.Write("6");
            qwer.Write("7");
            yxcv.Write("8");
            asdf.Enabled = true;
            yxcv.Enabled = true;
            qwer.Enabled = false;
            asdf.Write("9");
            qwer.Write("A");
            yxcv.Write("B");
            qwer.Enabled = true;
            asdf.Write("C");
            qwer.Write("D");
            yxcv.Write("E");

            // Get log
            var log = DisableAllAndGetLog();

            // Get categories
            var categories = log
                .Select(Entry.Parse)
                .Where(e => e != null)
                .Select(e => e.Message)
                .ToArray();
            var actual = string.Join("", categories);

            // Verify
            Assert.AreEqual("0124579BCDE", actual);
        }

    }
}
