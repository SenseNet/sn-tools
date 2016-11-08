using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class SessionReader : Reader
    {
        private readonly List<Reader> _readers;

        public SessionReader(IEnumerable<Reader> perAppDomainReaders)
        {
            _readers = perAppDomainReaders.ToList();
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            return new SessionReaderEnumerator(_readers);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_readers == null)
                return;
            foreach (var reader in _readers)
                reader.Dispose();
        }
    }
}
