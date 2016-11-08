using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    internal class SessionReaderEnumerator : IEnumerator<Entry>
    {
        private readonly List<Reader> _readers;
        private List<IEnumerator<Entry>> _enumerators;

        public SessionReaderEnumerator(List<Reader> _readers)
        {
            this._readers = _readers;
        }

        object IEnumerator.Current => Current;

        public void Reset()
        {
            throw new NotSupportedException();
        }
        public void Dispose()
        {
            //UNDONE: Dispose chain
        }

        public Entry Current => _enumerators.Count < 1 ? null : _enumerators[0].Current;

        public bool MoveNext()
        {
            if (_enumerators == null)
            {
                _enumerators = new List<IEnumerator<Entry>>();
                foreach (var reader in _readers)
                {
                    var enumerator = reader.GetEnumerator();
                    if (enumerator.MoveNext())
                        _enumerators.Add(enumerator);
                }
            }
            else {
                if (_enumerators.Count > 0 && !_enumerators[0].MoveNext())
                    _enumerators.RemoveAt(0);
            }
            _enumerators.Sort((x, y) => x.Current.Time.CompareTo(y.Current.Time));
            return _enumerators.Count > 0;
        }
    }
}