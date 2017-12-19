using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public class EntryCollection : Entry
    {
        public EntryCollection() : base() { }
        public EntryCollection(Entry sourceEntry) : base(sourceEntry) { }

        public virtual void Add(Entry entry, string qualification)
        {
        }

        public virtual bool Finished()
        {
            return true;
        }
    }

    public class GenericCollector<TSource, TResult> : IEnumerable<TResult> where TSource : Entry where TResult : EntryCollection, new()
    {
        private IEnumerable<Entry> _input;
        private readonly Func<TSource, Tuple<string, string>> _keySelector;
        private readonly Func<TResult, TResult> _finalizer;
        private static readonly Func<TResult, TResult> DefaultFinalizer = (c) => { return c.Finished() ? c : null; };

        public GenericCollector(Func<TSource, Tuple<string, string>> keySelector, Func<TResult, TResult> finalizer = null)
        {
            _keySelector = keySelector;
            _finalizer = finalizer ?? DefaultFinalizer;
        }
        public void Initialize(IEnumerable<Entry> input)
        {
            _input = input;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<TResult> GetEnumerator()
        {
            foreach (TSource entry in _input)
            {
                var keySelectorResult = _keySelector(entry);
                TResult collection;
                TResult outputEntry;
                if (keySelectorResult != null)
                {
                    var key = keySelectorResult.Item1;
                    var qualification = keySelectorResult.Item2;
                    collection = Collect<TResult>(key, qualification, entry);
                    if (collection != null)
                    {
                        outputEntry = _finalizer(collection);
                        //outputEntry = Build<TResult>(collection);
                        if (outputEntry != null)
                        {
                            RemoveCollection(key);
                            yield return outputEntry;
                        }
                    }
                }
            }
        }

        private T Collect<T>(string key, string qualification, Entry entry) where T : EntryCollection, new()
        {
            var collection = GetCollection<T>(key);
            collection.Add(entry, qualification);
            return collection;
        }

        private Dictionary<string, EntryCollection> Collections { get; } = new Dictionary<string, EntryCollection>();
        internal T GetCollection<T>(string key) where T : EntryCollection, new()
        {
            EntryCollection collection;
            if (!Collections.TryGetValue(key, out collection))
            {
                collection = new T();
                Collections[key] = collection;
            }
            return (T)collection;
        }
        internal void RemoveCollection(string key)
        {
            Collections.Remove(key);
        }

    }
}
