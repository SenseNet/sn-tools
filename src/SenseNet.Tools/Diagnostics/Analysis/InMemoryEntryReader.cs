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
            {
                var entry = Entry.Parse(item);
                if (entry != null)
                    yield return entry;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // do nothing
        }
    }
}
