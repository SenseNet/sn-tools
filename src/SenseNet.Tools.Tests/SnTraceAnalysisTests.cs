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


        private class AppDomainSimplifier
        {
            private readonly string _format;
            private List<string> _keys = new List<string>();

            public AppDomainSimplifier(string format = null)
            {
                _format = format ?? "App-{0}";
            }

            public string Simplify(string key)
            {
                var i = _keys.IndexOf(key);
                if (i < 0)
                {
                    i = _keys.Count;
                    _keys.Add(key);
                }
                return string.Format(_format, (i + 1));
            }
        }
        private class WebRequestEntryCollection : EntryCollection
        {
            public static class Q
            {
                public const string Start = "start";
                public const string End = "end";
            }

            public Entry StartEntry;
            public Entry EndEntry;

            public override void Add(Entry e, string qualification)
            {
                switch (qualification)
                {
                    case Q.Start:
                        StartEntry = e;
                        break;
                    case Q.End:
                        EndEntry = e;
                        break;
                }
            }

            public override bool Finished()
            {
                return StartEntry != null && EndEntry != null;
            }
        }
        #region string[] _logForSimpleCollectTest
        string[] _logForSimpleCollectTest = new[]
        {
            ">60\t2017-11-13 03:55:48.49992\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:5\t\t\t\tCPU: 0%, RAM: 680960 KBytes available (working set: 258744320 bytes)",
            ">61\t2017-11-13 03:55:53.51096\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:18\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/",
            "62\t2017-11-13 03:55:53.51096\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:18\t\t\t\tHTTP Action.ActionType: DefaultHttpAction, TargetNode: [null], AppNode: [null], RequestUrl:http://snbweb01.sn.hu/",
            "63\t2017-11-13 03:55:53.52746\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:18\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/, StatusCode:200",
            "64\t2017-11-13 03:55:53.52746\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:18\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/",
            ">65\t2017-11-13 03:55:53.65160\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:19\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/favicon.ico",
            "66\t2017-11-13 03:55:53.65160\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:19\t\t\t\tHTTP Action.ActionType: DefaultHttpAction, TargetNode: [null], AppNode: [null], RequestUrl:http://snbweb01.sn.hu/favicon.ico",
            "67\t2017-11-13 03:55:53.74574\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:19\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/favicon.ico, StatusCode:200",
            "68\t2017-11-13 03:55:53.74574\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:19\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/favicon.ico",
            ">69\t2017-11-13 03:55:58.50896\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550188456390176\tT:9\t\t\t\tCPU: 3,922418%, RAM: 570948 KBytes available (working set: 256319488 bytes)",
        };
        #endregion
        [TestMethod]
        public void SnTrace_Analysis_SimpleCollect()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(_logForSimpleCollectTest))
            {
                var aps = new AppDomainSimplifier("App{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web")
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect<Entry, WebRequestEntryCollection>((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", WebRequestEntryCollection.Q.Start);
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", WebRequestEntryCollection.Q.End);
                        return null;
                    })
                    .Take(50);
                foreach (var item in transformedLogFlow)
                {
                    var app = item.StartEntry.AppDomain;
                    var time = item.StartEntry.Time.ToString("HH:mm:ss.fffff");
                    var req = item.StartEntry.Message.Substring("PCM.OnEnter ".Length);
                    var dt = item.EndEntry.Time - item.StartEntry.Time;
                    writer.WriteLine($"{app}\t{time}\t{dt}\t{req}");
                }
            }

            // validate
            var line = new string[3];
            using (var reader = new StringReader(sb.ToString()))
            {
                // app   start-time      duration          request
                // ----  --------------  ----------------  -------------------------------------
                // App1  03:55:53.51096  00:00:00.0165000  GET http://snbweb01.sn.hu/
                // App1  03:55:53.65160  00:00:00.0941400  GET http://snbweb01.sn.hu/favicon.ico
                line[0] = reader.ReadLine();
                line[1] = reader.ReadLine();
                line[2] = reader.ReadLine();
            }
            Assert.IsNull(line[2]);
            var item0 = line[0].Split('\t');
            var item1 = line[1].Split('\t');

            Assert.AreEqual("App1", item0[0]);
            Assert.AreEqual("App1", item1[0]);
            Assert.AreEqual("03:55:53.51096", item0[1]);
            Assert.AreEqual("03:55:53.65160", item1[1]);
            Assert.AreEqual("00:00:00.0165000", item0[2]);
            Assert.AreEqual("00:00:00.0941400", item1[2]);
            Assert.AreEqual("GET http://snbweb01.sn.hu/", item0[3]);
            Assert.AreEqual("GET http://snbweb01.sn.hu/favicon.ico", item1[3]);
        }

    }
}
