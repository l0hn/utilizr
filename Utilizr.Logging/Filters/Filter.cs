using System;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Filters
{
    /// <summary>
    /// Filter instances are used to perform arbitrary filtering of LogRecords
    /// 
    /// Loggers and Handlers can optionally use Filter instances to filter records as desired. The base filter
    /// class only allows events which are below a certain point in the logger hierarchy. For example, a filter
    /// initialised with "A.B" will allow events logger by loggers "A.B", "A.B.C", "A.B.C.D", "A.B.D" etc. but
    /// not "A.BB", "B.A.B" etc. If initilised with an empty string, all events are passed.
    /// </summary>
    public class Filter : IFilter
    {
        private readonly string _category;

        public Filter(string category = "")
        {
            _category = category;
        }

        /// <summary>
        /// Determine if the specified record is to be logged.
        /// 
        /// Is the specified record to be logged?
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool FilterRecord(LogRecord record)
        {
            if (string.IsNullOrEmpty(_category))
                return true;
            if (_category == record.Category)
                return true;
            if (record.Category[.._category.Length] != _category)
                return false;
            return record.Category[_category.Length] == '.';
        }
    }
}
