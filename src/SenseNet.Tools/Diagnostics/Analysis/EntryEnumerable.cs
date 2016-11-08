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
    public abstract class EntryEnumerable<T> : IDisposable, IEnumerable<T> where T : Entry
    {
        public abstract void Dispose();

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public EntryEnumerable<Q> Filter<Q>(Func<Q, bool> decision) where Q : Entry
        {
            return new Filter<Q>(this, decision);
        }
        public EntryEnumerable<Q> Linker<Q>(Linker<Q> instance) where Q : Entry
        {
            instance.Initialize(this);
            return instance;
        }
        public EntryEnumerable<Q> Link<Q>(
            Func<Entry, Q> rootEntrySelector,
            Func<Entry, Q, LinkerState> lastEntrySelector,
            Action<Entry, Q> associator,
            Func<Q, Q> unfinishedEntrySelector
            ) where Q : Entry
        {
            var instance = new GenericLinker<Q>(rootEntrySelector, lastEntrySelector, associator, unfinishedEntrySelector);
            instance.Initialize(this);
            return instance;
        }
        public Transformer<T> Transformer<Q>(Transformer<T> instance) where Q : Entry
        {
            instance.Initialize(this);
            return instance;
        }
        public Transformer<T> Transform<Q>(Func<T, string> transformerMethod) where Q : Entry
        {
            var instance = new GenericTransformer<T>(transformerMethod);
            instance.Initialize(this);
            return instance;
        }
    }
}
