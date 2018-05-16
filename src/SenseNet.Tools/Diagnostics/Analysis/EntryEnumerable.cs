using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591 //UNDONE: write summaries and remove all #pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    public abstract class EntryEnumerable<T> : IDisposable, IEnumerable, IEnumerable<T> where T : Entry
    {
        public abstract void Dispose();

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
