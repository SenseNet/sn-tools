using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public abstract class Reader : EntryEnumerable<Entry>
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
        public static Reader Create(IEnumerable<string> directoryPaths, string filter)
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


        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
