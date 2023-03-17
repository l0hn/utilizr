using System.Collections.Generic;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// A handler class which buffers logging records in memory. Whenever each record is
    /// added to the buffer, a check is made to see if the buffer should be flushed.
    /// If it should, then flush() is expected to do what's needed.
    /// </summary>
    public abstract class BufferingHandler : Handler
    {
        public uint Capacity { get; set; }
        protected List<LogRecord> _buffer = new();

        protected BufferingHandler(uint capacity) : base()
        {
            Capacity = capacity;
        }

        /// <summary>
        /// Should the handler flush its buffer?
        /// 
        /// This method can be overridden to implement custom flushing strategies.
        /// </summary>
        /// <param name="record"></param>
        /// <returns>ture if the buffer is up to capacity.</returns>
        protected virtual bool ShouldFlush(LogRecord record)
        {
            return _buffer.Count >= Capacity;
        }

        /// <summary>
        /// Emit a record
        /// 
        /// Append the record to the buffer, if shouldFlush() tells us to, call flush() to 
        /// process the buffer.
        /// </summary>
        /// <param name="record"></param>
        protected override void Emit(LogRecord record)
        {
            _buffer.Add(record);

            if (ShouldFlush(record))
                Flush();
        }
    }
}
