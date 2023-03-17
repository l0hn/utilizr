using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Utilizr.Logging.Formatters;
using Utilizr.Logging.Handlers;
using Utilizr.Logging.Interfaces;
using Utilizr.Logging.Loggers;

namespace Utilizr.Logging
{
    public enum LoggingLevel : uint
    {
        CRITICAL = 50,
        ERROR = 40,
        WARNING = 30,
        INFO = 20,
        DEBUG = 10,
        NOTSET = 0,
    }


    public static class Log
    {
        internal static readonly RootLogger Root = new RootLogger(Defaults.Level);
        internal static readonly DateTime StartTime = DateTime.Now;

        public static bool Disabled
        {
            get { return Manager.Disabled; }
            set { Manager.Disabled = value; }
        }

        public static LoggingLevel Level
        {
            get { return Root.Level; }
            set { Root.Level = value; }
        }

        /// <summary>
        /// Basic configuration for the logging system.
        /// 
        /// This does nothing if the root logger already has handlers configured. It is a convenience method intended for use by 
        /// simple scripts and programs to do one-shot configuration of the logging. 
        /// 
        /// The default behaviour is to create a StreamHandler which writes to StandardError, sets a formatter using the Defaults.format 
        /// format string, and adds the handler to the root logger.
        /// </summary>
        /// <param name="logFilePath"></param>
        /// <param name="format"></param>
        /// <param name="dateFormat"></param>
        /// <param name="level"></param>
        public static void BasicConfigure(
            string logFilePath,
            string? format = null,
            string? dateFormat = null,
            LoggingLevel level = LoggingLevel.NOTSET,
            bool async = false)
        {
            if (Root.HandlerCount != 0)
                return;

            if (string.IsNullOrEmpty(logFilePath))
                throw new ArgumentException($"Null or empty value provided for {nameof(logFilePath)}");

            var handler = new RotatingFileHandler(logFilePath, 100*1024*1024, 2);
            BasicConfigure(handler, format, dateFormat, level, async);
        }


        /// <summary>
        /// Basic configuration for the logging system.
        /// 
        /// This does nothing if the root logger already has handlers configured. It is a convenience method intended for use by 
        /// simple scripts and programs to do one-shot configuration of the logging. 
        /// 
        /// The default behaviour is to create a StreamHandler which writes to StandardError, sets a formatter using the Defaults.format 
        /// format string, and adds the handler to the root logger.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <param name="dateFormat"></param>
        /// <param name="level"></param>
        public static void BasicConfigure(
            Stream stream,
            string? format = null,
            string? dateFormat = null,
            LoggingLevel level = LoggingLevel.NOTSET,
            bool async = false)
        {
            if (Root.HandlerCount != 0)
                return;

            if (stream == null)
                throw new ArgumentException($"Null passed for {nameof(stream)} parameter");

            var handler = new StreamHandler(stream);
            BasicConfigure(handler, format, dateFormat, level, async);
        }

        static void BasicConfigure(IHandler handler, string? format, string? dateFormat, LoggingLevel level = LoggingLevel.NOTSET, bool async = false)
        {
            format ??= Defaults.Format;
            dateFormat ??= Defaults.DateFormat;
            var formatter = new Formatter(format, dateFormat);
            handler.Formatter = formatter;

            Root.AddHandler(handler);
            if (Debugger.IsAttached)
                Root.AddHandler(new DebugHandler(formatter));

            if (level != LoggingLevel.NOTSET)
                Root.Level = level;

            Root.Async = async;
        }

        /// <summary>
        /// Add the specified handler to the root logger.
        /// </summary>
        /// <param name="handler"></param>
        public static void AddHandler(IHandler handler)
        {
            Root.AddHandler(handler);
        }

        /// <summary>
        /// Remove the specified handler from the root logger.
        /// </summary>
        /// <param name="handler"></param>
        public static void RemoveHandler(IHandler handler)
        {
            Root.RemoveHandler(handler);
        }

        public static Logger GetLogger(string name)
        {
            return Manager.GetLogger(name);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity DEBUG to the root logger.
        /// 
        /// Log.debug("Houston, we have a {0}", "thorny problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Debug(string message, params object[] args)
        {
            Root.Debug(message, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity DEBUG to getLogger(category)
        /// 
        /// Log.debug("Houston, we have a {0}", "thorny problem");
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Debug(string category, string message, params object[] args)
        {
            GetLogger(category).Debug(message, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity INFO to the root logger.
        /// 
        /// Log.info("Houston, we have a {0}", "interesting problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Info(string message, params object[] args)
        {
            Root.Info(message, args);
        }

        /// <summary>
        /// Log String.Format(message, args) with severity INFO to getLogger(category)
        /// 
        /// Log.info("Houston, we have a {0}", "interesting problem");
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Info(string category, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Info(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity WARNING to the root logger.
        /// 
        /// Log.warning("Houston, we have a {0}", "bit of a problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Warning(string message, params object[] args)
        {
            try
            {
                Root.Warning(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity WARNING to getLogger(category).
        /// 
        /// Log.warning("Houston, we have a {0}", "bit of a problem");
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Warning(string category, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Warning(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity ERROR to the root logger.
        /// 
        /// Log.error("Houston, we have a {0}", "major problem");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Error(string message, params object[] args)
        {
            try
            {
                Root.Error(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity ERROR to getLogger(category).
        /// 
        /// Log.error("Houston, we have a {0}", "major problem");
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Error(string category, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Error(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity CRITICAL to the root logger.
        /// 
        /// Logs.critical("Houston, we have a {0}", "major disaster");
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Critical(string message, params object[] args)
        {
            try
            {
                Root.Critical(message, args);
            }
            catch 
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with severity CRITICAL to getLogger(category)
        /// 
        /// Logs.critical("Houston, we have a {0}", "major disaster");
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Critical(string category, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Critical(message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level to the root logger
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Message(LoggingLevel level, string message, params object[] args)
        {
            try
            {
                Root.Message(level, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level to getLogger(category).
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Message(LoggingLevel level, string category, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Message(level, message, args);
            }
            catch 
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level to the root logger, also with exception information.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Exception(LoggingLevel level, Exception error, string message = "", params object[] args)
        {
            try
            {
                Root.Exception(level, error, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the LoggingLevel.ERROR to the root logger, also with exception information.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Exception(Exception error, string message = "", params object[] args)
        {
            try
            {
                Root.Exception(error, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, also with exception information to getLogger(category).
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Exception(LoggingLevel level, string category, Exception error, string message = "", params object[] args)
        {
            try
            {
                GetLogger(category).Exception(level, error, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with LoggingLevel.ERROR, also with exception information to getLogger(category).
        /// </summary>
        /// <param name="category"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Exception(string category, Exception error, string message = "",
            params object[] args)
        {
            try
            {
                GetLogger(category).Exception(error, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level to the root logger, also with any objects to log as well.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="interestingObjects"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Objects(LoggingLevel level, object[] interestingObjects, string message, params object[] args)
        {
            try
            {
                Root.Objects(level, interestingObjects, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, aslo with any objects to log as well to getLogger(category).
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="interestingObjects"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Objects(LoggingLevel level, string category, object[] interestingObjects, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Objects(level, interestingObjects, message, args);
            }
            catch
            {

            }
        }


        /// <summary>
        /// Log String.Format(message, args) with the specified severity level to the root logger, also extra values to be passed to the formatter
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extra"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Extra(LoggingLevel level, Dictionary<string, object> extra, string message, params object[] args)
        {
            try
            {
                Root.Extra(level, extra, message, args);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Log String.Format(message, args) with the specified severity level, also extra values to be passed to the formatter to getLogger(category).
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="extra"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void Extra(LoggingLevel level, string category, Dictionary<string, object> extra, string message, params object[] args)
        {
            try
            {
                GetLogger(category).Extra(level, extra, message, args);
            }
            catch (Exception)
            {

            }
        }
    }
}
