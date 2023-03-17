using System;
using System.Collections.Generic;
using System.IO;
using Utilizr.Logging.Loggers;

namespace Utilizr.Logging
{
    internal static class Manager
    {
        internal static bool Disabled { get; set; }
        internal static Dictionary<string, Logger> LoggerDict { get; set; }
        internal static Dictionary<string, PlaceHolder> PlaceholderDict { get; set; }

        private static bool _madeNoHandlerWarning;
        private static object LOCK = new object();

        static Manager()
        {
            LoggerDict = new Dictionary<string, Logger>();
            PlaceholderDict = new Dictionary<string, PlaceHolder>();
        }

        internal static Logger GetLogger(string category)
        {
            Logger logger;
            lock (LOCK)
            {
                if (LoggerDict.ContainsKey(category))
                {
                    logger = LoggerDict[category];
                }
                else if (PlaceholderDict.TryGetValue(category, out PlaceHolder? placeHolder))
                {
                    logger = new Logger(category);
                    LoggerDict[category] = logger;
                    FixupChildren(placeHolder, logger);
                    FixupParents(logger);
                    PlaceholderDict.Remove(category);
                }
                else
                {
                    logger = new Logger(category);
                    LoggerDict[category] = logger;
                    FixupParents(logger);
                }
                return logger;
            }
        }

        /// <summary>
        /// Ensure that there are either loggers or placeholders all the way from the specified logger
        /// to the root of the logger hierarchy.
        /// </summary>
        /// <param name="logger"></param>
        private static void FixupParents(Logger logger)
        {
            string category = logger.Category;
            int i = category.LastIndexOf('.');
            Logger? parent = null;
            while (i != -1 && parent == null)
            {
                string subString = category.Substring(0, i);
                if (!LoggerDict.ContainsKey(subString) && !PlaceholderDict.ContainsKey(subString))
                {
                    PlaceholderDict[subString] = new PlaceHolder(logger);
                }
                else
                {
                    LoggerDict.TryGetValue(subString, out Logger? outLogger);
                    if (outLogger != null)
                    {
                        parent = outLogger;
                    }
                    else
                    {
                        var placeholder = PlaceholderDict[subString];
                        placeholder.Add(logger);
                    }
                }
                i = subString.LastIndexOf('.');
            }
            parent ??= Log.Root;
            logger.Parent = parent;
        }

        /// <summary>
        /// Ensure that children of the placeholder are connected to the specified logger.
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="logger"></param>
        private static void FixupChildren(PlaceHolder placeholder, Logger logger)
        {
            string category = logger.Category;
            foreach (Logger child in placeholder.loggerMap.Keys)
            {
                if (!child.Parent?.Category.StartsWith(category) == true)
                {
                    logger.Parent = child.Parent;
                    child.Parent = logger;
                }
            }
        }

        internal static void NoHandlers(string name)
        {
            lock (LOCK)
            {
                if (!_madeNoHandlerWarning)
                {
                    Stream stderr = Console.OpenStandardError();
                    StreamWriter writer = new StreamWriter(stderr);
                    writer.WriteLine(string.Format("No handlers could be found for logger \"{0}\"", name));
                    writer.Close();
                    _madeNoHandlerWarning = true;
                }
            }
        }
    }

    /// <summary>
    /// PlaceHolder instances are used in the Manager logger hierarchy to take the place of nodes for which
    /// no loggers have been defined.
    /// </summary>
    class PlaceHolder
    {
        internal Dictionary<Logger, bool> loggerMap = new Dictionary<Logger,bool>();

        /// <summary>
        /// Initialises with the specified logger being a child of this placehotlder.
        /// </summary>
        /// <param name="logger"></param>
        internal PlaceHolder(Logger logger)
        {
            loggerMap[logger] = true;
        }

        /// <summary>
        /// Add the specified logger as a child of this placeholder
        /// </summary>
        /// <param name="logger"></param>
        internal void Add(Logger logger)
        {
            if (!loggerMap.ContainsKey(logger))
            {
                loggerMap[logger] = true;
            }
        }
    }
}
