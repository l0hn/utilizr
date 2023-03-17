using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Utilizr.Logging
{
    /// <summary>
    /// A logRecord instance represents an event being logged.
    /// </summary>
    public class LogRecord
    {
        public DateTime Created { get; private set; }
        public string Category { get; private set; }
        public LoggingLevel Level { get; private set; }
        public Exception Error { get; private set; }
        public string? ErrorText { get; set; }  //Used to cache traceback text
        public TimeSpan RelativeCreated { get; private set; }
        public string? Asctime { get; set; }
        public Dictionary<string, object> Extra { get; private set; }

        private string? _message;
        public string Message
        {
            get
            {
                _message ??= _args?.Length > 0 == true
                    ? string.Format(_fmtMessage, _args)
                    : _fmtMessage;
                return _message;
            }
        }

        private string? _interestingObjects;
        public string InterestingObjects
        {
            get
            {
                _interestingObjects ??= JsonConvert.SerializeObject(_interestingObjectsRaw);
                return _interestingObjects;
            }
        }

        public string LevelName => Level.ToString();

        private readonly string _fmtMessage;
        private readonly object[]? _args;
        private readonly object[]? _interestingObjectsRaw;

        internal LogRecord(
            string category,
            LoggingLevel level,
            string message,
            Exception? error = null,
            object[]? interestingObjects = null,
            Dictionary<string, object>? extra = null,
            params object[] args)
        {
            Created = DateTime.UtcNow;
            Category = category;
            Level = level;
            _fmtMessage = message;
            Error = error!;
            Extra = extra!;
            _args = args;
            _interestingObjectsRaw = interestingObjects!;
            RelativeCreated = Created - Log.StartTime;
        }
    }
}