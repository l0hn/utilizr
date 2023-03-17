using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// A handler class which buffers logging records in memory, periodically flushing them to a 
    /// target handler. Flushing occurs whenever the buffer is full, or when an event of a certain
    /// severity of greater is seen.
    /// </summary>
    public class MemoryHandler : BufferingHandler
    {
        protected LoggingLevel _flushLevel;
        public IHandler? Target { get; private set; }

        /// <summary>
        /// Initialise with the buffer size, the level at which flushing should occur and an optional target.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="flushLevel"></param>
        /// <param name="target">Note that without a target being set either here or via the property this wont
        /// do anything useful!</param>
        public MemoryHandler(uint capacity, LoggingLevel flushLevel = LoggingLevel.ERROR, IHandler? target = null)
            : base(capacity)
        {
            _flushLevel = flushLevel;
            Target = target;
        }

        /// <summary>
        /// Check if the buffer is full or a record is at the flushLevel or higher
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected override bool ShouldFlush(LogRecord record)
        {
            return base.ShouldFlush(record) || record.Level >= _flushLevel;
        }

        /// <summary>
        /// Flush the buffered records to the target, if there is one.
        /// </summary>
        public override void Flush()
        {
            if (Target == null) return;
            foreach (var record in _buffer)
            {
                Target.Handle(record);
            }
            _buffer.Clear();
        }

        /// <summary>
        /// Flush the buffer and clear it.
        /// </summary>
        public override void Close()
        {
            Flush();
        }
    }
}