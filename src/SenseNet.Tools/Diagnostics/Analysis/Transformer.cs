using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Transformer<T> : IDisposable, IEnumerable<string> where T : Entry
    {
        private IEnumerable<Entry> _input;
        internal void Initialize(IEnumerable<Entry> input)
        {
            _input = input;
        }

        public IEnumerator<string> GetEnumerator()
        {
            string output;
            foreach (T item in _input)
                if ((output = Transform(item)) != null)
                    yield return output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract string Transform(T entry);


        public void Dispose()
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
