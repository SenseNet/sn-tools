using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Linker<T> : EntryEnumerable<T> where T : Entry
    {
        protected IEnumerable<Entry> _input;

        internal void Initialize(IEnumerable<Entry> input)
        {
            _input = input;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            T output;
            foreach (var item in _input)
                if ((output = Process(item)) != null)
                    yield return output;
        }

        protected abstract T Process(Entry input);

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            var disposable = _input as IDisposable;
            disposable?.Dispose();
        }
    }
}