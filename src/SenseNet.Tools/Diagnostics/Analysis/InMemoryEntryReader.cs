using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public class InMemoryEntryReader : Reader
    {
        IEnumerable<string> _entrySource;

        public InMemoryEntryReader(IEnumerable<string> entrySource)
        {
            _entrySource = entrySource;
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            foreach (var item in _entrySource)
                yield return Entry.Parse(item);
        }

        protected override void Dispose(bool disposing)
        {
            // do nothing
        }
    }
}
