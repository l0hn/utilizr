using System.Collections.Generic;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Filters
{
    /// <summary>
    /// Base class for loggers and handlers which allow them to share common code.
    /// </summary>
    public class Filterer : IFilter
    {
        private readonly List<IFilter> _filters = new();
        protected object LOCK = new();

        public LoggingLevel Level { get; set; }

        public Filterer()
        {
            Level = LoggingLevel.NOTSET;
        }

        /// <summary>
        /// Add the specified filter to this handler
        /// </summary>
        /// <param name="filter"></param>
        public void AddFilter(IFilter filter)
        {
            if (!_filters.Contains(filter))
                _filters.Add(filter);
        }

        /// <summary>
        /// Remove the specified filter from this handler
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveFilter(IFilter filter)
        {
            if (_filters.Contains(filter))
                _filters.Remove(filter);
        }

        /// <summary>
        /// Determine if a record is loggable by consulting all the filters.
        /// 
        /// The default is to allow the record to be logged; any filter can veto this and the record is the dropped.
        /// Returns false if the record is to be dropped, else true.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool FilterRecord(LogRecord record)
        {
            foreach (IFilter filter in _filters)
            {
                if (!filter.FilterRecord(record))
                    return false;
            }
            return true;
        }
    }
}
