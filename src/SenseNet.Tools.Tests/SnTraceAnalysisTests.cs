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
        //[TestMethod]
        //public void SnTrace_Analysis_SimpleApi_Filter()
        //{
        //    CleanupAndEnableAll();
        //    WriteStructure1();

        //    using (var logFlow = Reader.Create(base.DetailedLogDirectory))
        //    {
        //        var transformedLogFlow = logFlow
        //            .Where(e => e.Status == Status.End)
        //            .Where(e => e.Category == Category.Test);

        //        var actual = string.Join(", ", transformedLogFlow.Select(e => e.Message));
        //        Assert.AreEqual("10, 7, 14", actual);
        //    }
        //}
        //private void WriteStructure1()
        //{
        //    SnTrace.Test.Write("1");
        //    SnTrace.Write("2");
        //    SnTrace.Test.Write("3");
        //    SnTrace.Write("4");
        //    SnTrace.Web.Write("5");
        //    SnTrace.Repository.Write("6");
        //    using (var op = SnTrace.Test.StartOperation("7"))
        //    {
        //        using (var op1 = SnTrace.StartOperation("8"))
        //        {
        //            op1.Successful = true;
        //        }
        //        using (var op2 = SnTrace.Repository.StartOperation("9"))
        //        {
        //            op2.Successful = true;
        //        }
        //        using (var op2 = SnTrace.Test.StartOperation("10"))
        //        {
        //            op2.Successful = true;
        //        }
        //        using (var op2 = SnTrace.Test.StartOperation("11"))
        //        {
        //            op2.Successful = false;
        //        }
        //        op.Successful = true;
        //    }
        //    SnTrace.Test.Write("12");
        //    SnTrace.Repository.Write("13");
        //    using (var op2 = SnTrace.Test.StartOperation("14"))
        //    {
        //        op2.Successful = true;
        //    }
        //    SnTrace.Repository.Write("15");
        //    SnTrace.Test.Write("16");

        //    SnTrace.Flush();
        //}


        //private void WriteStructure2()
        //{
        //    SnTrace.Write("noise"); Wait(1);
        //    SnTrace.Write("noise"); Wait(1);
        //    SnTrace.Test.Write("noise"); Wait(1);
        //    SnTrace.Write("noise"); Wait(1);

        //    SnTrace.Test.Write("TEST START"); Wait(1);               // relevant

        //    SnTrace.Write("noise"); Wait(1);
        //    SnTrace.Test.Write("noise"); Wait(1);
        //    SnTrace.Write("noise"); Wait(1);
        //    using (var op1 = SnTrace.Test.StartOperation("Op1"))
        //    {
        //        SnTrace.Test.Write("noise"); Wait(1);
        //        SnTrace.Write("noise"); Wait(1);
        //        SnTrace.Test.Write("noise"); Wait(1);
        //        op1.Successful = true;
        //    }                                                        // relevant
        //    SnTrace.Write("noise"); Wait(1);
        //    using (var op2 = SnTrace.Test.StartOperation("Op2"))
        //    {
        //        SnTrace.Test.Write("noise"); Wait(1);
        //        SnTrace.Write("noise"); Wait(1);
        //        SnTrace.Test.Write("noise"); Wait(1);
        //        op2.Successful = true;
        //    }                                                        // relevant

        //    SnTrace.Test.Write("TEST END"); Wait(1);

        //    SnTrace.Write("noise"); Wait(1);
        //    SnTrace.Test.Write("noise"); Wait(1);

        //    SnTrace.Test.Write("TEST START"); Wait(1); // (irrelevant)

        //    SnTrace.Flush();
        //}
        //private void Wait(int milleseconds)
        //{
        //    System.Threading.Thread.Sleep(milleseconds);
        //}

        //[TestMethod]
        //public void SnTrace_Analysis_FluentApi_FunctionalStyle()
        //{
        //    CleanupAndEnableAll();
        //    WriteStructure2();

        //    var output = new StringBuilder();
        //    using (var writer = new StringWriter(output))
        //    using (var logFlow = Reader.Create(base.DetailedLogDirectory))
        //    {
        //        var transformedLogFlow = logFlow
        //            .Where<Entry>(e => e.Message.StartsWith("TEST ") || e.Status == Status.End)
        //            .Take(3);
        //        foreach (var line in transformedLogFlow)
        //            writer.WriteLine(line);
        //    }

        //    var times = output.ToString().Split('\t').Select(TimeSpan.Parse).ToArray();
        //    Assert.AreEqual(3, times.Length);
        //    Assert.IsTrue(times[0] + times[1] < times[2]);
        //}


        //private class TestEntry : Entry
        //{
        //    public TimeSpan Op1Duration { get; set; }
        //    public TimeSpan Op2Duration { get; set; }
        //    public TimeSpan GeneralDuration { get; set; }

        //    public TestEntry(Entry sourceEntry) : base(sourceEntry) { }
        //}

    }
}
