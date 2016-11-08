using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Filter<T> : EntryEnumerable<T> where T : Entry
    {
        private readonly IEnumerable<Entry> _inputEntries;
        private readonly Func<T, bool> _decision;
        public Filter(IEnumerable<Entry> inputEntries, Func<T, bool> decision)
        {
            _inputEntries = inputEntries;
            _decision = decision;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            foreach (T entry in _inputEntries)
                if (_decision(entry))
                    yield return entry;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            var disposable = _inputEntries as IDisposable;
            disposable?.Dispose();
        }
    }
}
