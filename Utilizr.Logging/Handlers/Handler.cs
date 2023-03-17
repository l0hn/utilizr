using System;
using Utilizr.Logging.Filters;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// Handler instances dispatch logging events to specific destinations.
    /// </summary>
    public abstract class Handler : Filterer, IHandler, IDisposable
    {
        public IFormatter? Formatter { get; set; }

        /// <summary>
        /// Initialised with null formatter and an empty filter list.
        /// </summary>
        public Handler()
            : base()
        {
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
            if (log)
            {
                lock (LOCK)
                {
                    HandleEmit(record);
                }
            }
            return log;
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
                string.Format("There was a problem logging to {0} from {1}.{2}{3}",
                    record.Category,
                    this.GetType().Name,
                    Environment.NewLine,
                    error
                )
            );
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
