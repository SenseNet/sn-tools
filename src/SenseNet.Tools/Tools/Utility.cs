using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <summary>
    /// A helper class containing common utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Contains converter methods.
        /// </summary>
        public static class Convert
        {
            /// <summary>
            /// Converts an array of bytes to a long value.
            /// </summary>
            /// <param name="bytes">The byte array to convert.</param>
            public static long BytesToLong(byte[] bytes)
            {
                return bytes.Aggregate(0L, (current, t) => (current << 8) + t);
            }

            /// <summary>
            /// Converts a long value to an array of bytes.
            /// </summary>
            /// <param name="long">The long value to convert.</param>
            public static byte[] LongToBytes(long @long)
            {
                var bytes = new byte[8];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[7 - i] = (byte)(@long & 0xFF);
                    @long = @long >> 8;
                }
                return bytes;
            }
        }

        /// <summary>
        /// Walks through an inner exception chain of the provided exception
        /// and collects all the messages, properties and stack trace lines
        /// into a single string.
        /// </summary>
        /// <param name="ex">An exception to crawl.</param>
        public static string CollectExceptionMessages(Exception ex)
        {
            var sb = new StringBuilder();

            sb.Append(ex.GetType().Name).Append(": ").AppendLine(ex.Message);
            PrintTypeLoadError(ex as ReflectionTypeLoadException, sb);
            sb.AppendLine(ex.StackTrace);
            while ((ex = ex.InnerException) != null)
            {
                sb.AppendLine("---- Inner Exception:");
                sb.Append(ex.GetType().Name);
                sb.Append(": ");
                sb.AppendLine(ex.Message);
                PrintTypeLoadError(ex as ReflectionTypeLoadException, sb);
                sb.AppendLine(ex.StackTrace);
            }
            sb.AppendLine("=====================");

            return sb.ToString();
        }
        private static void PrintTypeLoadError(ReflectionTypeLoadException exc, StringBuilder sb)
        {
            if (exc == null)
                return;
            sb.AppendLine("LoaderExceptions:");
            foreach (var e in exc.LoaderExceptions)
            {
                sb.Append("-- ");
                sb.Append(e.GetType().FullName);
                sb.Append(": ");
                sb.AppendLine(e.Message);

                var fileNotFoundException = e as FileNotFoundException;
                if (fileNotFoundException != null)
                {
                    sb.AppendLine("FUSION LOG:");
                    sb.AppendLine(fileNotFoundException.FusionLog);
                }
            }
        }

        /// <summary>
        /// Collects the list of types involved in the type load exception. Looks for
        /// information in the LoaderExceptions property, recognizing <see cref="FileLoadException"/>,
        /// <see cref="FileNotFoundException"/>, <see cref="BadImageFormatException"/>, 
        /// <see cref="SecurityException"/> and <see cref="TypeLoadException"/>.
        /// </summary>
        public static IEnumerable<string> GetTypeLoadErrorTypes(ReflectionTypeLoadException ex)
        {
            var types = new List<string>();
            if (ex?.LoaderExceptions == null)
                return types;

            foreach (var item in ex.LoaderExceptions)
            {
                switch (item)
                {
                    case FileLoadException flo:
                        types.Add(flo.FileName);
                        break;
                    case FileNotFoundException f:
                        types.Add(f.FileName);
                        break;
                    case BadImageFormatException b:
                        types.Add(b.FileName);
                        break;
                    case SecurityException s:
                        types.Add(s.Url);
                        break;
                    case TypeLoadException tl:
                        types.Add(tl.TypeName);
                        break;
                }
            }

            return types;
        }
    }
}
