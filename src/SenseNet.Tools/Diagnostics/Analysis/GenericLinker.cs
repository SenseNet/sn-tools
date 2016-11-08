using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GenericLinker<T> : Linker<T> where T : Entry
    {
        private readonly Func<Entry, T> _rootEntrySelector;
        private readonly Func<Entry, T, LinkerState> _lastEntrySelector;
        private readonly Action<Entry, T> _associator;
        private readonly Func<T, T> _unfinishedEntrySelector;

        private readonly Dictionary<string, T> _records = new Dictionary<string, T>();

        public GenericLinker(
            Func<Entry, T> rootEntrySelector,
            Func<Entry, T, LinkerState> lastEntrySelector,
            Action<Entry, T> associator,
            Func<T, T> unfinishedEntrySelector
            )
        {
            _rootEntrySelector = rootEntrySelector;
            _lastEntrySelector = lastEntrySelector;
            _associator = associator;
            _unfinishedEntrySelector = unfinishedEntrySelector;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            T output;
            foreach (var item in _input)
                if ((output = Process(item)) != null)
                    yield return output;

            if (_unfinishedEntrySelector != null)
                foreach (var item in _records.Values.OrderBy(r => r.Time))
                    if ((output = _unfinishedEntrySelector(item)) != null)
                        yield return output;
        }

        protected override T Process(Entry input)
        {
            var key = input.AppDomain + "_T:" + input.ThreadId;

            var record = _rootEntrySelector(input);
            if (record != null)
            {
                _records[key] = record;
            }
            else
            {
                if (!_records.TryGetValue(key, out record))
                    return null;
            }

            var state = _lastEntrySelector(input, record);
            if (state != LinkerState.NotLast)
                return state == LinkerState.LastComplete ? record : null;
            _associator?.Invoke(input, record);
            return null;
        }
    }
}
