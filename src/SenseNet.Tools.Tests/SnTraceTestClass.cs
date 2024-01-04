#define SIMPLIFIED_TRACE_LINE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using SenseNet.Diagnostics.Analysis;

namespace SenseNet.Tools.Tests
{
    public abstract class SnTraceTestClass
    {
        // ReSharper disable once InconsistentNaming
        private string __detailedLogDirectory;
        protected string DetailedLogDirectory => __detailedLogDirectory ??
                                                 (__detailedLogDirectory = SnFileSystemTracer.GetRelativeLogDirectory(AppDomain.CurrentDomain.BaseDirectory));

        protected void ResetOperationId()
        {
            SnTrace.Operation.ResetOperationId();
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

#if SIMPLIFIED_TRACE_LINE
            return fields.Length < 7 ? null : string.Join("\t", fields, 6, fields.Length - 6);
#else
            return fields.Length < 10 ? null : string.Join("\t", fields, 9, fields.Length - 9);
#endif
        }
        protected string GetColumnFromLine(string line, Entry.Field col)
        {
            var fields = line?.Split('\t');
            return fields?[(int)col];
        }

    }
}
