using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilizr.Logging.Filters;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Loggers
{
    /// <summary>
    /// Instances of the Logger class represent a single logging channel.
    /// A "logging channel" indicates an area of an application.
    /// Exactly how "area" is defined is up to the application developer.
    /// </summary>
    public class Logger : Filterer, IDisposable
    {
        internal RootLogger? Root { get; set; }
        internal Logger? Parent { get; set; }

        public string Category { get; set; }
        public bool Propagate { get; set; }
        public bool Disabled { get; set; }
        public bool Async { get; set; }

        /// <summary>
        /// List of all handlers added to the Logger
        /// </summary>
        public List<IHandler> Handlers { get; private set; }

        /// <summary>
        /// Number of handlers added to the logger
        /// </summary>
        public int HandlerCount => Handlers.Count;


        private readonly ConcurrentQueue<LogRecord> _queuedLogRecords;

        public Logger(string category, LoggingLevel level = LoggingLevel.NOTSET, bool async = false) : base()
        {
            Category = category;
            Level = level;
            Propagate = true;
            Async = async;
            _queuedLogRecords = new ConcurrentQueue<LogRecord>();
            Handlers = new List<IHandler>();
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning); // todo: ensure not using threadpool thread
        }

        /// <summary>
        /// Log String.Format(message, args) with severity DEBUG
        /// 
        /// logger.debug("Houston, we have a {0}", "thorny problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Debug(string message, params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.DEBUG))
                LogImpl(LoggingLevel.DEBUG, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity INFO
        /// 
        /// logger.info("Houston, we have a {0}", "interesting problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Info(string message, params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.INFO))
                LogImpl(LoggingLevel.INFO, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity WARNING
        /// 
        /// logger.warning("Houston, we have a {0}", "bit of a problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Warning(string message, params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.WARNING))
                LogImpl(LoggingLevel.WARNING, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity ERROR
        /// 
        /// logger.error("Houston, we have a {0}", "major problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Error(string message, params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.ERROR))
                LogImpl(LoggingLevel.ERROR, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity CRITICAL
        /// 
        /// logger.critical("Houston, we have a {0}", "major disaster");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Critical(string message, params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.CRITICAL))
                LogImpl(LoggingLevel.CRITICAL, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity, error information and any objects to log as well.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <param name="interestingObjects"></param>
        /// <param name="args"></param>
        public void Log(LoggingLevel level, string message, Exception? error = null, object[]? interestingObjects = null, Dictionary<string, object>? extra = null, params object[] args)
        {
            if (IsEnabledFor(level))
                LogImpl(level, message, error, interestingObjects, extra, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Message(LoggingLevel level, string message, params object[] args)
        {
            if (IsEnabledFor(level))
                LogImpl(level, message, null, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, also with exception information.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Exception(LoggingLevel level, Exception error, string message = "", params object[] args)
        {
            if (IsEnabledFor(level))
                LogImpl(level, message, error, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with LoggingLevel.ERROR, also with exception information.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Exception(Exception error, string message = "", params object[] args)
        {
            if (IsEnabledFor(LoggingLevel.ERROR))
                LogImpl(LoggingLevel.ERROR, message, error, null, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, also with any objects to log as well.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="interestingObjects"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Objects(LoggingLevel level, object[] interestingObjects, string message, params object[] args)
        {
            if (IsEnabledFor(level))
                LogImpl(level, message, null, interestingObjects, null, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, also extra values to be passed to the formatter.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extra"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void Extra(LoggingLevel level, Dictionary<string, object> extra, string message, params object[] args)
        {
            if (IsEnabledFor(level))
                LogImpl(level, message, null, null, extra, args);
        }

        protected void LogImpl(
            LoggingLevel level, 
            string message, 
            Exception? error, 
            object[]? interestingObjects,
            Dictionary<string, object>? extra,
            params object[] args)
        {
            var record = new LogRecord(Category, level, message, error, interestingObjects, extra, args);
            if (Async)
            {
                QueueLogRecord(record);
                return;
            }

            Handle(record);
        }

        protected void Handle(LogRecord record)
        {
            if (!Disabled && FilterRecord(record))
                CallHandlers(record);
        }

        protected void QueueLogRecord(LogRecord record)
        {
            if (!Disabled && FilterRecord(record))
                _queuedLogRecords.Enqueue(record);
        }

        private void ProcessQueue()
        {
            while (true)
            {
                try
                {
                    if (!_queuedLogRecords.TryDequeue(out var nextLogRecord))
                    {
                        Task.Delay(100).Wait();
                        continue;
                    }

                    Handle(nextLogRecord);
                }
                catch (Exception)
                {
                    Task.Delay(1000).Wait();
                }
            }
        }

        /// <summary>
        /// Add the specified handler to this logger.
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(IHandler handler)
        {
            lock (LOCK)
            {
                if (!Handlers.Contains(handler))
                    Handlers.Add(handler);
            }
        }

        /// <summary>
        /// Remove the specified handler from this logger.
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandler(IHandler handler)
        {
            lock (LOCK)
            {
                if (Handlers.Contains(handler))
                    Handlers.Remove(handler);
            }
        }

        protected void CallHandlers(LogRecord record)
        {
            Logger? logger = this;
            int found = 0;
            while (logger != null)
            {
                lock (logger.LOCK)
                {
                    foreach (IHandler handler in logger.Handlers)
                    {
                        found++;
                        if (record.Level >= handler.Level)
                            handler.Handle(record);
                    }
                }

                if (!logger.Propagate)
                    logger = null;
                else
                    logger = logger.Parent;
            }

            if (found == 0)
                Manager.NoHandlers(record.Category);
        }

        protected LoggingLevel EffectiveLevel
        {
            get
            {
                Logger? logger = this;
                while (logger != null)
                {
                    if (logger.Level != LoggingLevel.NOTSET)
                        return logger.Level;
                    logger = logger!.Parent;
                }
                return LoggingLevel.NOTSET;
            }
        }

        public bool IsEnabledFor(LoggingLevel level)
        {
            if (Manager.Disabled)
                return false;

            return level >= EffectiveLevel;
        }

        public Logger GetChild(string suffix)
        {
            if (!ReferenceEquals(Root, this))
                suffix = Category + "." + suffix;

            return Manager.GetLogger(suffix);
        }

        public void Dispose()
        {
            foreach (var handler in Handlers)
            {
                handler.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
