﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading;

// ReSharper disable UseStringInterpolation
// ReSharper disable StringLiteralTypo

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Efficient and scalable tracing component. Provides methods for recording 
    /// verbose information about the running system. Collects messages into a buffer 
    /// and writes them to the file system periodically.
    /// This class cannot be inherited.
    /// </summary>
    public static class SnTrace
    {
        //====================================================================== Nested classes

        /// <summary>
        /// Represents an execution block that needs a start and finish log message
        /// regardless of whether the execution was successful or not. The two
        /// messages can be associated by their common operation id. 
        /// Setting the Successful property to true is mandatory when the execution
        /// was successful - otherwise the logger would assume that the operation failed.
        /// Typical usage:
        /// using (var op = SnTrace.StartOperation("message")) { ...; op.Successful = true; }
        /// </summary>
        public class Operation : IDisposable
        {
            private static long _nextId = 1;

            internal static readonly Operation Null = new Operation(0L);

            internal static void ResetOperationId(long operationId = 1)
            {
                _nextId = operationId;
            }

            /// <summary>
            /// Gets the operation identifier that is unique in the current AppDomain.
            /// </summary>
            public long Id { get; }

            /// <summary>
            /// Gets the category name.
            /// </summary>
            public string Category { get; }
            /// <summary>
            /// Gets the time when the operation started.
            /// </summary>
            public DateTime StartedAt { get; internal set; }
            /// <summary>
            /// Gets the operation message that is written at start and at the end.
            /// </summary>
            public string Message { get; internal set; }
            /// <summary>
            /// Gets or sets a value indicating whether the operation is finished correctly.
            /// Always set this flag to true when the code block executed correctly.
            /// Default is false.
            /// </summary>
            public bool Successful { get; set; }

            private Operation(long id)
            {
                Id = id;
                Category = string.Empty;
            }
            internal Operation(string category)
            {
                Id = Interlocked.Increment(ref _nextId) - 1;
                Category = category;
            }

            private void Finish()
            {
                if (this != Null)
                    WriteEndToLog(this);
            }

            /// <summary>
            /// Finishes the operation and writes the trace line containing the message and the running time.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// Releases the unmanaged resources used by the Operation object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">True to release both managed and unmanaged resources or false to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                    Finish();
            }
        }

        /// <summary>
        /// Represents an SnTrace category. It helps differentiating trace lines 
        /// that are generated by different features.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public class SnTraceCategory
        {
            /// <summary>
            /// Gets the name of the category.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets or sets whether the category is enabled or not. Messages sent to 
            /// a disabled category will not be written to the trace log.
            /// </summary>
            public bool Enabled { get; set; }

            internal SnTraceCategory(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Starts a traced operation in the current category. The message will be written 
            /// to the trace with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            /// <returns>A disposable operation object usually encapsulated in a using block.</returns>
            public Operation StartOperation(string message, params object[] args)
            {
                return Enabled ? StartOp(Name, message, args) : Operation.Null;
            }

            /// <summary>
            /// Starts a traced operation in the current category. The message will be written to the trace.
            /// </summary>
            /// <param name="getMessage">A function that returns the message to log. It will be executed
            /// only if the category is enabled.</param>
            /// <returns>A disposable operation object usually encapsulated in a using block.</returns>
            public Operation StartOperation(Func<string> getMessage)
            {
                return StartOperation(Enabled ? getMessage() : string.Empty);
            }

            /// <summary>
            /// Writes a line to the trace with the current category. The message will be written with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            public void Write(string message, params object[] args)
            {
                if (!Enabled)
                    return;
                Log(Name, false, message, args);
            }
            /// <summary>
            /// Writes a line to the trace with the current category.
            /// </summary>
            /// <param name="getMessage">A function that returns the message to log. It will be executed
            /// only if the category is enabled.</param>
            public void Write(Func<string> getMessage)
            {
                if (!Enabled || getMessage == null)
                    return;
                Write(getMessage());
            }
            /// <summary>
            /// Writes an error line to the trace with the current category. The message will be written with smart formatting.
            /// </summary>
            /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
            /// <param name="args">Parameters that will be substituted into the message template.
            /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
            /// All control characters (including tabs, return and line feed) are changed to '.'
            /// </param>
            public void WriteError(string message, params object[] args)
            {
                if (!Enabled)
                    return;
                Log(Name, true, message, args);
            }
            /// <summary>
            /// Writes an error line to the trace with the current category.
            /// </summary>
            /// <param name="getMessage">A function that returns the message to log. It will be executed
            /// only if the category is enabled.</param>
            public void WriteError(Func<string> getMessage)
            {
                if (!Enabled || getMessage == null)
                    return;
                WriteError(getMessage());
            }
        }

        //====================================================================== Built-in categories

        /// <summary>ContentOperation category</summary>
        public static readonly SnTraceCategory ContentOperation = new SnTraceCategory("ContentOperation");
        /// <summary>Database category</summary>
        public static readonly SnTraceCategory Database = new SnTraceCategory("Database");
        /// <summary>Index category</summary>
        public static readonly SnTraceCategory Index = new SnTraceCategory("Index");
        /// <summary>IndexQueue category</summary>
        public static readonly SnTraceCategory IndexQueue = new SnTraceCategory("IndexQueue");
        /// <summary>Query category</summary>
        public static readonly SnTraceCategory Query = new SnTraceCategory("Query");
        /// <summary>Repository category</summary>
        public static readonly SnTraceCategory Repository = new SnTraceCategory("Repository");
        /// <summary>Messaging category</summary>
        public static readonly SnTraceCategory Messaging = new SnTraceCategory("Messaging");
        /// <summary>Security category</summary>
        public static readonly SnTraceCategory Security = new SnTraceCategory("Security");
        /// <summary>SecurityDatabase category</summary>
        public static readonly SnTraceCategory SecurityDatabase = new SnTraceCategory("SecurityDatabase");
        /// <summary>SecurityQueue category</summary>
        public static readonly SnTraceCategory SecurityQueue = new SnTraceCategory("SecurityQueue");
        /// <summary>System category</summary>
        public static readonly SnTraceCategory System = new SnTraceCategory("System");
        /// <summary>Web category</summary>
        public static readonly SnTraceCategory Web = new SnTraceCategory("Web");
        /// <summary>Workflow category</summary>
        public static readonly SnTraceCategory Workflow = new SnTraceCategory("Workflow");
        /// <summary>TaskManagement category</summary>
        public static readonly SnTraceCategory TaskManagement = new SnTraceCategory("TaskManagement");
        /// <summary>Test category</summary>
        public static readonly SnTraceCategory Test = new SnTraceCategory("Test");
        /// <summary>Event category</summary>
        public static readonly SnTraceCategory Event = new SnTraceCategory("Event");
        /// <summary>Custom category</summary>
        public static readonly SnTraceCategory Custom = new SnTraceCategory("Custom");

        /// <summary>
        /// Contains all SnTrace categories to help enumerate them.
        /// </summary>
        public static readonly SnTraceCategory[] Categories = { ContentOperation, Database, Index, IndexQueue, Query, Repository, Messaging, Security, SecurityDatabase, SecurityQueue, System, Web, Workflow, TaskManagement, Test, Event, Custom };

        //====================================================================== Static API

        /// <summary>
        /// Creates a dynamic trace category.
        /// </summary>
        /// <param name="name">Category name.</param>
        /// <returns>A category object that is enabled (meaning messages written into it will be persisted) if the Custom category is enabled.</returns>
        public static SnTraceCategory Category(string name)
        {
            return new SnTraceCategory(name) { Enabled = Custom.Enabled };
        }

        /// <summary>
        ///  Starts a traced operation in the "Custom" category. The message will be written to the trace with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        /// <returns>A disposable operation object usually encapsulated in a using block.</returns>
        public static Operation StartOperation(string message, params object[] args)
        {
            return Custom.StartOperation(message, args);
        }
        /// <summary>
        ///  Starts a traced operation in the "Custom" category.
        /// </summary>
        /// <param name="getMessage">A function that returns the message to log. It will be executed
        /// only if the category is enabled.</param>
        /// <returns>A disposable operation object usually encapsulated in a using block.</returns>
        public static Operation StartOperation(Func<string> getMessage)
        {
            return Custom.StartOperation(getMessage);
        }
        /// <summary>
        /// Writes a line to the trace in the "Custom" category. The message will be written with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        public static void Write(string message, params object[] args)
        {
            Custom.Write(message, args);
        }
        /// <summary>
        /// Writes a line to the trace in the "Custom" category.
        /// </summary>
        /// <param name="getMessage">A function that returns the message to log. It will be executed
        /// only if the category is enabled.</param>
        public static void Write(Func<string> getMessage)
        {
            Custom.Write(getMessage);
        }
        /// <summary>
        /// Writes an error line to the trace in the "Custom" category. The message will be written with smart formatting.
        /// </summary>
        /// <param name="message">Message template that works as a composite format string (see string.Format method).</param>
        /// <param name="args">Parameters that will be substituted into the message template.
        /// Null values will be written as "null". IEnumerable values will be written as comma separated lists.
        /// All control characters (including tabs, return and line feed) are changed to '.'
        /// </param>
        public static void WriteError(string message, params object[] args)
        {
            Custom.WriteError(message, args);
        }
        /// <summary>
        /// Writes an error line to the trace in the "Custom" category.
        /// </summary>
        /// <param name="getMessage">A function that returns the message to log. It will be executed
        /// only if the category is enabled.</param>
        public static void WriteError(Func<string> getMessage)
        {
            Custom.WriteError(getMessage);
        }

        /// <summary>
        /// Enables all categories.
        /// </summary>
        public static void EnableAll()
        {
            foreach (var snTraceCategory in Categories)
            {
                snTraceCategory.Enabled = true;
            }
        }

        /// <summary>
        /// Disables all categories.
        /// </summary>
        public static void DisableAll()
        {
            foreach (var snTraceCategory in Categories)
            {
                snTraceCategory.Enabled = false;
            }
        }

        /// <summary>
        /// Clears all buffers in the providers and causes any buffered data to be written to the underlying devices.
        /// </summary>
        public static void Flush()
        {
            foreach (var provider in SnTracers)
                provider.Flush();
        }

        //====================================================================== Buffer and Operation

        private static Operation StartOp(string category, string message, params object[] args)
        {
            var op = new Operation(category) {StartedAt = DateTime.UtcNow};

            // protection against unprintable characters
            var line = SafeFormatString(category, false, op, message, args);

            WriteToProviders(line);

            return op;
        }
        internal static void Log(string category, bool isError, string message, params object[] args)
        {
            // protection against unprintable characters
            var line = SafeFormatString(category, isError, null, message, args);

            WriteToProviders(line);
        }

        private static void WriteEndToLog(Operation op)
        {
            var line = FinishOperation(op);
            WriteToProviders(line);
        }


        // ReSharper disable once InconsistentNaming
        private static string __appDomainName;
        private static string AppDomainName => __appDomainName ??= Guid.NewGuid().ToString();

        /*================================================================== ProgramFlow */

        private static long _nextProgramFlowId = 1L;
        private static readonly AsyncLocal<long> ProgramFlow = new AsyncLocal<long>();

        private static long GetProgramFlowId()
        {
            if (ProgramFlow.Value == 0)
                ProgramFlow.Value = Interlocked.Increment(ref _nextProgramFlowId) - 1;
            return ProgramFlow.Value;
        }

        /*================================================================== Logger */

        /// <summary>
        /// Gets or sets the trace provider implementation instances.
        /// </summary>
        public static List<ISnTracer> SnTracers { get; } = new List<ISnTracer>(new[] { new SnDebugViewTracer() });
        private static void WriteToProviders(string line)
        {
            foreach (var provider in SnTracers)
                provider.Write(line);
        }


        private static string Escape(string input)
        {
            var c = input.ToCharArray();
            for (var i = 0; i < c.Length; i++)
                if (c[i] < ' ' && c[i] != '\t')
                    c[i] = '.';
            return new string(c);
        }

        private static int _lineCounter;

        private static string FinishOperation(Operation op)
        {
            var lineCounter = Interlocked.Increment(ref _lineCounter);
            var programFlow = GetProgramFlowId();

            var line = string.Format("{0}\t{1:yyyy-MM-dd HH:mm:ss.fffff}\t{2}\tA:{3}\tT:{4}\tPf:{5}\tOp:{6}\t{7}\t{8:hh\':\'mm\':\'ss\'.\'ffffff}\t{9}"
                , lineCounter
                , DateTime.UtcNow, op.Category
                , AppDomainName
                , Thread.CurrentThread.ManagedThreadId
                , programFlow
                , op.Id
                , op.Successful ? "End" : "UNTERMINATED"
                , DateTime.UtcNow - op.StartedAt
                , op.Message);

            return line;
        }

        private static string SafeFormatString(string category, bool isError, Operation op, string message, params object[] args)
        {
            var lineCounter = Interlocked.Increment(ref _lineCounter);
            var programFlow = GetProgramFlowId();
            var line = op != null
                ? string.Format("{0}\t{1:yyyy-MM-dd HH:mm:ss.fffff}\t{2}\tA:{3}\tT:{4}\tPf:{5}\tOp:{6}\tStart\t\t"
                    , lineCounter
                    , DateTime.UtcNow
                    , category
                    , AppDomainName
                    , Thread.CurrentThread.ManagedThreadId
                    , programFlow
                    , op.Id)
                : string.Format("{0}\t{1:yyyy-MM-dd HH:mm:ss.fffff}\t{2}\tA:{3}\tT:{4}\tPf:{5}\t\t{6}\t\t"
                    , lineCounter
                    , DateTime.UtcNow
                    , category
                    , AppDomainName
                    , Thread.CurrentThread.ManagedThreadId
                    , programFlow
                    , isError ? "ERROR" : "");

            // smart formatting
            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case string _:
                        case IDictionary _:
                            continue;
                        case null:
                            args[i] = "[null]";
                            continue;
                    }

                    if (!(arg is IEnumerable enumerable))
                        continue;

                    var sb = new StringBuilder("[");
                    foreach (var item in enumerable)
                    {
                        if (sb.Length > 1)
                            sb.Append(", ");
                        sb.Append(item);
                    }
                    sb.Append("]");
                    args[i] = sb.ToString();
                }
            }

            string msg;
            try
            {
                msg = Escape(string.Format(message, args ?? new object[0]));
            }
            catch (Exception e)
            {
                msg = Escape($"SNTRACE ERROR: {message}. {e.Message}");

                var thisModule = new StackTrace().GetFrame(0).GetMethod().Module;
                MethodBase callerMethod;
                var stackTrace = new StackTrace();

                for(var i = 2; ;i++)
                {
                    callerMethod = stackTrace.GetFrame(i).GetMethod();
                    if (thisModule != callerMethod.Module)
                        break;
                }

                msg += $". Caller: {callerMethod}, " + 
                    $"(type: {callerMethod.DeclaringType?.FullName ?? "?"}, " + 
                    $"asm: {callerMethod.Module.Name})";
            }

            line += msg;

            if (op != null)
                op.Message = msg;

            return line;
        }
    }
}