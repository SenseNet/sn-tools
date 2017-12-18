using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public abstract class Reader : IEnumerable<Entry>, IDisposable
    {
        public static Reader Create(string path)
        {
            if (File.Exists(path))
                return new FileReader(path);
            if (Directory.Exists(path))
                return new DirectoryReader(path);
            throw new InvalidOperationException("Specified path does not exist.");
        }
        public static Reader Create(string directoryPath, string filter)
        {
            if (Directory.Exists(directoryPath))
                return new DirectoryReader(directoryPath, filter);
            throw new InvalidOperationException("Specified directory does not exist.");
        }
        public static Reader Create(string[] directoryPaths, string filter)
        {
            var readers = new List<DirectoryReader>();

            foreach (var directoryPath in directoryPaths)
            {
                if (Directory.Exists(directoryPath))
                    readers.Add(new DirectoryReader(directoryPath, filter));
                else
                    throw new InvalidOperationException("Specified directory does not exist: " + directoryPath);
            }

            return new SessionReader(readers);
        }
        public static Reader Create(IEnumerable<string> entrySource)
        {
            return new InMemoryEntryReader(entrySource);
        }
        public static Reader Create(IEnumerable<IEnumerable<string>> entrySources)
        {
            return new SessionReader(entrySources.Select(e => new InMemoryEntryReader(e)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(bool disposing);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public abstract IEnumerator<Entry> GetEnumerator();
    }
}
