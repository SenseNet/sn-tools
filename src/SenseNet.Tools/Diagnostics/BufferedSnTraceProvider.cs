using System;
using System.Text;
using System.Threading;

namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Defines a base class for the buffered SnTrace writers.
    /// Implements the ISnTraceProvider.
    /// </summary>
    public abstract class BufferedSnTraceProvider : ISnTraceProvider
    {
        private const int DefaultGufferSize = 10000;
        private const int DefaultWriteDelay = 1000;

        private string[] _buffer = new string[DefaultGufferSize];
        private long _bufferSize = DefaultGufferSize;
        private long _bufferPosition; // this field is incremented by every logger thread.
        private long _lastBufferPosition; // this field is written by only CollectLines method.
        private long _writeDelay = DefaultWriteDelay;
        private long _blockSizeWarning;

        /// <summary>Statistical data: the longest gap between p0 and p1</summary>
        private long _maxPdiff;

        private Timer _timer;

        /// <summary>
        /// Initializes the buffer and the timer.
        /// </summary>
        /// <param name="bufferSize">Defines what count of lines can be buffered. If the buffers full,
        /// the new line overrides the oldest line and "BUFFER OVERRUN ERROR" message will be written. Default value is 10000.</param>
        /// <param name="writeDelay">Time between two writing in milliseconds. Default value is 1000.</param>
        protected void Initialize(long bufferSize, int writeDelay)
        {
            _bufferSize = bufferSize;
            _blockSizeWarning = Math.Max(bufferSize / 5, 100);
            _buffer = new string[_bufferSize];
            _writeDelay = writeDelay;

            _timer = new Timer(_ => TimerTick(), null, _writeDelay, _writeDelay);
        }

        /// <summary>
        /// Writes the given line into a buffer.
        /// </summary>
        public virtual void Write(string line)
        {
            // writing to the buffer
            var p = Interlocked.Increment(ref _bufferPosition) - 1;
            _buffer[p % _bufferSize] = line;
        }

        private readonly object _writeSync = new object();
        private void TimerTick()
        {
            lock (_writeSync)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite); //stops the timer

                var text = CollectLines();
                if (text != null)
                    WriteBatch(text.ToString());

                _timer.Change(_writeDelay, _writeDelay); //restart
            }
        }
        private StringBuilder CollectLines()
        {
            var p0 = _lastBufferPosition;
            var p1 = Interlocked.Read(ref _bufferPosition);

            if (p0 == p1)
                return null;

            var sb = new StringBuilder(">"); // the '>' sign means: block writing start.
            var pdiff = p1 - p0;
            if (pdiff > _maxPdiff)
                _maxPdiff = pdiff;


            if (pdiff > _bufferSize)
                sb.AppendFormat("BUFFER OVERRUN ERROR: Buffer size is {0}, unwritten lines : {1}", _bufferSize, pdiff).AppendLine();

            while (p0 < p1)
            {
                var p = p0 % _bufferSize;
                var line = _buffer[p];
                sb.AppendLine(line);
                p0++;
            }

            _lastBufferPosition = p1;

            // If the block contains more than 20% of the buffer size, write a message
            if (pdiff > _blockSizeWarning)
                sb.AppendFormat("Block size reaches the risky limit: {0}. Buffer size: {1}", pdiff, _bufferSize).AppendLine();

            return sb;
        }

        /// <summary>
        /// Writes all buffered lines to the filesystem and empties the buffer.
        /// </summary>
        public virtual void Flush()
        {
            TimerTick();
        }

        /// <summary>
        /// Writes all buffered lines to the filesystem.
        /// </summary>
        protected abstract void WriteBatch(string message);
    }
}
