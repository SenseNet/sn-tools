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
        [TestMethod]
        public void SnTrace_Analysis_SimpleApi_Filter()
        {
            CleanupAndEnableAll();
            WriteStructure1();

            using (var reader = Reader.Create(base.DetailedLogDirectory))
            using (var statusFilter = new Filter<Entry>(reader, e => e.Status == Status.End))
            using (var categoryFilter = new Filter<Entry>(statusFilter, e => e.Category == Category.Test))
            {
                var actual = string.Join(", ", categoryFilter.Select(e => e.Message));
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


        [TestMethod]
        public void SnTrace_Analysis_FluentApi_FilterLinkTransform()
        {
            CleanupAndEnableAll();
            WriteStructure2();

            using (var analyzator = Reader.Create(base.DetailedLogDirectory)
                .Filter<Entry>(e => e.Message.StartsWith("TEST ") || e.Status == Status.End)
                .Linker<TestEntry>(new TestLinker())
                .Transformer<TestEntry>(new TestTransformer())
                )
            {
                var result = analyzator.ToArray();

                Assert.AreEqual(1, result.Length);
                var times = result[0].Split('\t').Select(TimeSpan.Parse).ToArray();
                Assert.IsTrue(times[0] + times[1] < times[2]);
            }
        }
        private void WriteStructure2()
        {
            SnTrace.Write("noise"); Wait(1);
            SnTrace.Write("noise"); Wait(1);
            SnTrace.Test.Write("noise"); Wait(1);
            SnTrace.Write("noise"); Wait(1);

            SnTrace.Test.Write("TEST START"); Wait(1);               // relevant

            SnTrace.Write("noise"); Wait(1);
            SnTrace.Test.Write("noise"); Wait(1);
            SnTrace.Write("noise"); Wait(1);
            using (var op1 = SnTrace.Test.StartOperation("Op1"))
            {
                SnTrace.Test.Write("noise"); Wait(1);
                SnTrace.Write("noise"); Wait(1);
                SnTrace.Test.Write("noise"); Wait(1);
                op1.Successful = true;
            }                                                        // relevant
            SnTrace.Write("noise"); Wait(1);
            using (var op2 = SnTrace.Test.StartOperation("Op2"))
            {
                SnTrace.Test.Write("noise"); Wait(1);
                SnTrace.Write("noise"); Wait(1);
                SnTrace.Test.Write("noise"); Wait(1);
                op2.Successful = true;
            }                                                        // relevant

            SnTrace.Test.Write("TEST END"); Wait(1);

            SnTrace.Write("noise"); Wait(1);
            SnTrace.Test.Write("noise"); Wait(1);

            SnTrace.Test.Write("TEST START"); Wait(1); // (irrelevant)

            SnTrace.Flush();
        }
        private void Wait(int milleseconds)
        {
            System.Threading.Thread.Sleep(milleseconds);
        }


        [TestMethod]
        public void SnTrace_Analysis_FluentApi_Writer()
        {
            CleanupAndEnableAll();
            WriteStructure2();

            var output = new StringBuilder();
            using (var writer = new StringWriter(output))
            using (var analyzator = Reader.Create(base.DetailedLogDirectory)
                .Filter<Entry>(e => e.Message.StartsWith("TEST ") || e.Status == Status.End)
                .Linker<TestEntry>(new TestLinker())
                .Transformer<TestEntry>(new TestTransformer())
                )
            {
                foreach (var line in analyzator)
                    writer.WriteLine(line);
            }
            var times = output.ToString().Split('\t').Select(TimeSpan.Parse).ToArray();
            Assert.AreEqual(3, times.Length);
            Assert.IsTrue(times[0] + times[1] < times[2]);
        }


        [TestMethod]
        public void SnTrace_Analysis_FluentApi_FunctionalStyle()
        {
            CleanupAndEnableAll();
            WriteStructure2();

            var output = new StringBuilder();
            using (var writer = new StringWriter(output))
            using (var analyzator = Reader.Create(base.DetailedLogDirectory)
                .Filter<Entry>(e => e.Message.StartsWith("TEST ") || e.Status == Status.End)
                .Link<TestEntry>(
                    rootEntrySelector: input => input.Message == "TEST START" ? new TestEntry(input) : null,
                    lastEntrySelector: (input, record) =>
                    {
                        if (input.Message != "TEST END")
                            return LinkerState.NotLast;
                        record.GeneralDuration = input.Time - record.Time;
                        return LinkerState.LastComplete;
                    },
                    associator: (input, record) =>
                    {
                        if (input.Status != Status.End)
                            return;
                        if (input.Message == "Op1")
                            record.Op1Duration = input.Duration;
                        if (input.Message == "Op2")
                            record.Op2Duration = input.Duration;
                    },
                    unfinishedEntrySelector: null
                )
                .Transformer<TestEntry>(new TestTransformer())
                )
            {
                foreach (var line in analyzator)
                    writer.WriteLine(line);
            }
            var times = output.ToString().Split('\t').Select(TimeSpan.Parse).ToArray();
            Assert.AreEqual(3, times.Length);
            Assert.IsTrue(times[0] + times[1] < times[2]);
        }


        private class TestEntry : Entry
        {
            public TimeSpan Op1Duration { get; set; }
            public TimeSpan Op2Duration { get; set; }
            public TimeSpan GeneralDuration { get; set; }

            public TestEntry(Entry sourceEntry) : base(sourceEntry) { }
        }

        private class TestLinker : Linker<TestEntry>
        {
            private readonly Dictionary<string, TestEntry> _records = new Dictionary<string, TestEntry>();
            protected override TestEntry Process(Entry input)
            {
                if (input.Category != Category.Test && input.Status != Status.End)
                    return null;

                var key = input.AppDomain + "_T:" + input.ThreadId;

                TestEntry record = null;
                if (input.Message == "TEST START")
                {
                    // start new scope: create new entry and override old if exists
                    record = new TestEntry(input);
                    _records[key] = record;
                }
                else
                {
                    if (!_records.TryGetValue(key, out record))
                        return null;
                }


                if (input.Status == Status.End)
                {
                    switch (input.Message)
                    {
                        case "Op1":
                            record.Op1Duration = input.Duration;
                            return null;
                        case "Op2":
                            record.Op2Duration = input.Duration;
                            return null;
                        default:
                            Assert.Inconclusive();
                            break;
                    }
                }
                else if (input.Message == "TEST END")
                {
                    record.GeneralDuration = input.Time - record.Time;
                    _records.Remove(key);
                    return record;
                }
                return null;
            }
        }

        private class TestTransformer : Transformer<TestEntry>
        {
            public override string Transform(TestEntry entry)
            {
                return String.Format("{0}\t{1}\t{2}", entry.Op1Duration, entry.Op2Duration, entry.GeneralDuration);
            }
        }

    }
}
