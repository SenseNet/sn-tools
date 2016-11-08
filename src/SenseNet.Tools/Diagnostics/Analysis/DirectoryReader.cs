using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class DirectoryReader : Reader
    {
        private readonly string _directoryPath;
        private readonly string _searchPattern;
        private FileReader[] _readerSequence;
        internal DirectoryReader(string directoryPath, string searchPattern = null)
        {
            _directoryPath = directoryPath;
            _searchPattern = searchPattern;
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            var files = _searchPattern == null
                ? Directory.GetFiles(_directoryPath)
                : Directory.GetFiles(_directoryPath, _searchPattern);

            _readerSequence = files
                .OrderBy(p => p)
                .Select(p => new FileReader(p))
                .ToArray();

            foreach (var reader in _readerSequence)
                foreach (var entry in reader)
                    yield return entry;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_readerSequence == null)
                return;
            foreach (var reader in _readerSequence)
                reader.Dispose();
        }
    }
}
