using System;
using System.Collections.Concurrent;
using System.Threading;
using Utilizr.Logging.Filters;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// Handler instances dispatch logging events to specific destinations.
    /// </summary>
    public abstract class Handler : Filterer, IHandler, IDisposable
    {
        private bool _async;
        private bool _disposed;
        private Thread? _processThread;
        private AutoResetEvent _queueReady;
        private ConcurrentQueue<LogRecord> _queuedLogRecords;

        public IFormatter? Formatter { get; set; }

        /// <summary>
        /// Initialised with null formatter and an empty filter list.
        /// </summary>
        public Handler(bool async = false)
            : base()
        {
            _async = async;
            _queueReady = new AutoResetEvent(false);
            _queuedLogRecords = new ConcurrentQueue<LogRecord>();
            if (async)
            {
                _processThread = new Thread(ProcessQueue)
                {
                    IsBackground = true,
                    Name = "LogQueueProcessor",
                };
                _processThread.Start();
            }
        }

        /// <summary>
        /// Initialised with null formatter and an empty filter list. Only logs messages with at least the LoggingLevel specified.
        /// </summary>
        /// <param name="level"></param>
        public Handler(LoggingLevel level)
            : this()
        {
            Level = level;
        }

        /// <summary>
        /// Format the specified record
        ///
        /// If a formatter is set, it is used otherwise the default formatter is used (Defaults.formatter)
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected string Format(LogRecord record)
        {
            var fmt = Formatter ?? Defaults.Formatter;
            return fmt.Format(record);
        }

        protected abstract void Emit(LogRecord record);

        protected void HandleEmit(LogRecord record)
        {
            try
            {
                Emit(record);
            }
            catch (Exception error)
            {
                HandleError(record, error);
            }
        }

        /// <summary>
        /// Conditionally emit the specified logging record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool Handle(LogRecord record)
        {
            bool log = FilterRecord(record);
            if (!log)
                return false;

            if (_async) {
                QueueLogRecord(record);
                return true;
            }

            lock (LOCK)
            {
                HandleEmit(record);
            }
            return true;
        }

        public abstract void Flush();

        public abstract void Close();

        /// <summary>
        /// Handle errors which occur during an emit() call.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="error"></param>
        protected void HandleError(LogRecord record, Exception error)
        {
            Console.Error.WriteLine(
                string.Format(
                    "There was a problem logging to {0} from {1}.{2}{3}",
                    record.Category,
                    this.GetType().Name,
                    Environment.NewLine,
                    error
                )
            );
        }

        protected void QueueLogRecord(LogRecord record)
        {
            if (!_disposed)
            {
                _queuedLogRecords.Enqueue(record);
                try
                {
                    _queueReady.Set();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        private void ProcessQueue()
        {
            while (!_disposed)
            {
                try
                {
                    if (!_queuedLogRecords.TryDequeue(out var nextLogRecord))
                    {
                        _queueReady.WaitOne();
                        continue;
                    }

                    HandleEmit(nextLogRecord);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
            _queuedLogRecords?.Clear();
            _queueReady?.Set();
            _queueReady?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
