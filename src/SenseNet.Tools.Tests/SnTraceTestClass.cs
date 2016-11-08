using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Tools.Tests
{
    public abstract class SnTraceTestClass
    {
        public TestContext TestContext { get; set; }


        private string __detailedLogDirectory;
        protected string DetailedLogDirectory => __detailedLogDirectory ??
                                                 (__detailedLogDirectory = SnTrace.GetRelativeLogDirectory(AppDomain.CurrentDomain.BaseDirectory));

        protected void ResetOperationId()
        {
            var acc = new PrivateType(typeof(SnTrace.Operation));
            acc.SetStaticField("_nextId", 1L);
        }

        private void CleanupLog()
        {
            if (!Directory.Exists(DetailedLogDirectory))
                return;
            foreach (var path in Directory.GetFiles(DetailedLogDirectory, "*.*"))
                File.Delete(path);
        }
        private List<string> GetLog()
        {
            var paths = Directory.GetFiles(DetailedLogDirectory, "*.*");
            var lines = new List<string>();
            string line;
            foreach (var path in paths)
                using (var reader = new StreamReader(path))
                    while ((line = reader.ReadLine()) != null)
                        if (line != "----")
                            lines.Add(line);
            return lines;
        }

        protected void CleanupAndEnableAll()
        {
            CleanupLog();
            SnTrace.EnableAll();
        }
        protected List<string> DisableAllAndGetLog()
        {
            SnTrace.Flush();
            SnTrace.DisableAll();
            return GetLog();
        }
        protected string GetMessageFromLine(string line)
        {
            if (line == null)
                return null;

            var fields = line.Split('\t');

            // 1	2	3	4	5	6	7	8	9:asdf
            return fields.Length < 9 ? null : string.Join("\t", fields, 8, fields.Length - 8);
        }
        protected string GetColumnFromLine(string line, int col)
        {
            var fields = line?.Split('\t');
            return fields?[col];
        }

    }
}
