using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Formatters
{
    /// <summary>
    /// A formatter suitable for formatting a number of records
    /// </summary>
    public class BufferingFormatter : IBufferingFormatter
    {
        private readonly IFormatter _lineFormat;

        /// <summary>
        /// Use the default formatter to format each individual record (Defaults.formatter)
        /// </summary>
        public BufferingFormatter()
        {
            _lineFormat = Defaults.Formatter;
        }

        /// <summary>
        /// Specify a formatter which will be used to format each individual record.
        /// </summary>
        /// <param name="formatter"></param>
        public BufferingFormatter(IFormatter formatter)
        {
            _lineFormat = formatter;
        }

        /// <summary>
        /// Return the header string for the specified records
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        string FormatHeader(LogRecord[] records)
        {
            return string.Empty;
        }

        /// <summary>
        /// Return the footer string for the specified records
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        string FormatFooter(LogRecord[] records)
        {
            return string.Empty;
        }

        /// <summary>
        /// Format the specified records
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        public string Format(LogRecord[] records)
        {
            string returnValue = "";
            if (records.Length > 0)
            {
                returnValue += FormatHeader(records);
                foreach (LogRecord record in records)
                    returnValue += _lineFormat.Format(record);
                returnValue += FormatFooter(records);
            }
            return returnValue;
        }
    }
}
