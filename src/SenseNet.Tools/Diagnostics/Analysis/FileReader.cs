using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class FileReader : Reader
    {
        private readonly string _filePath;
        private StreamReader _reader;
        internal FileReader(string filePath)
        {
            _filePath = filePath;
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            _reader = new StreamReader(_filePath);

            string line;
            while ((line = _reader.ReadLine()) != null)
                if (line.Length > 0 && !line.StartsWith("--") && !line.StartsWith("MaxPdiff:"))
                    yield return Entry.Parse(line);
        }

        protected override void Dispose(bool disposing)
        {
            IDisposable disposable;
            if (!disposing)
                return;
            if (_reader == null)
                return;
            if ((disposable = _reader as IDisposable) != null)
                disposable.Dispose();
        }
    }
}
