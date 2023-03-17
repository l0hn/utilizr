using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilizr.Logging.Handlers
{
    public class TimedRotatingFileHandler : BaseRotatingHandler
    {
        public DateTime RollOverAt { get; private set; }
        private DateTime CurrentTime => _utc ? DateTime.UtcNow : DateTime.Now;

        protected TimeSpan _when;
        protected bool _utc;
        protected string _suffix;
        protected Regex _matchExtension;

        public TimedRotatingFileHandler(string filePath, Encoding? encoding, TimeSpan? when = null, int backupCount = 0, bool delay = false, bool utc = false)
            : base(filePath, encoding, true, delay)
        {
            if (when == null)
                when = new TimeSpan(1, 0, 0);

            _when = (TimeSpan)when;
            _backupCount = backupCount;
            _utc = utc;

            var timeFileCreated = File.Exists(filePath)
                ? utc
                    ? File.GetCreationTimeUtc(filePath)
                    : File.GetCreationTime(filePath)
                : CurrentTime;

            RollOverAt = timeFileCreated + _when;

            string pattern;
            if (when < new TimeSpan(0, 1, 0))  //seconds
            {
                _suffix = "yyyy-MM-dd_HH-mm-ss";
                pattern = @"^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$";
            }
            else if (when < new TimeSpan(1, 0, 0))  //minutes
            {
                _suffix = "yyyy-MM-dd_HH-mm";
                pattern = @"^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}$";
                RollOverAt = new DateTime(RollOverAt.Year, RollOverAt.Month, RollOverAt.Day, RollOverAt.Hour, RollOverAt.Minute, 0, RollOverAt.Kind); // Make it rollover on the minute.
            }
            else if (when < new TimeSpan(1, 0, 0, 0))  //hours
            {
                _suffix = "yyyy-MM-dd_HH";
                pattern = @"^\d{4}-\d{2}-\d{2}_\d{2}$";
                RollOverAt = new DateTime(RollOverAt.Year, RollOverAt.Month, RollOverAt.Day, RollOverAt.Hour, 0, 0, RollOverAt.Kind); // Make it rollover on the hour.
            }
            else  //days
            {
                _suffix = "yyyy-MM-dd";
                pattern = @"^\d{4}-\d{2}-\d{2}$";
                RollOverAt = new DateTime(RollOverAt.Year, RollOverAt.Month, RollOverAt.Day, 0, 0, 0, RollOverAt.Kind);  // Make it rollover at midnight.
            }

            _matchExtension = new Regex(pattern);
        }

        public TimedRotatingFileHandler(string filePath, TimeSpan? when = null, int backupCount = 0, bool delay = false, bool utc = false)
            : this(filePath, null, when, backupCount, delay, utc)
        { }


        protected override bool ShouldRollover(LogRecord record)
        {
            if (_utc)
                return DateTime.UtcNow > RollOverAt;
            else
                return DateTime.Now > RollOverAt;
        }

        protected IEnumerable<string> GetFilePathsToDelete()
        {
            var dirName = Path.GetDirectoryName(FilePath);
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var extension = Path.GetExtension(FilePath);

            var paths = new List<string>();

            var prefix = fileName + ".";
            string suffix;

            foreach (var path in Directory.GetFiles(dirName!))
            {
                string targetFileName = Path.GetFileNameWithoutExtension(path);
                string targetExtension = Path.GetExtension(path);
                if ((targetFileName[..Math.Min(prefix.Length, targetFileName.Length)] == prefix) && (extension == targetExtension))
                {
                    suffix = targetFileName[prefix.Length..];
                    if (_matchExtension.Match(suffix).Success)
                        paths.Add(path);
                }
            }

            paths.Sort();

            if (paths.Count >= _backupCount)
                foreach (string item in paths.GetRange(0, paths.Count - _backupCount))
                    yield return item;
        }

        protected override void DoRollover()
        {
            _writer?.Close();
            DateTime time = RollOverAt - _when;

            string destFilePath = Path.Combine(
                Path.GetDirectoryName(FilePath)!,
                $"{Path.GetFileNameWithoutExtension(FilePath)}.{time.ToString(_suffix)}{Path.GetExtension(FilePath)}"
            );

            if (File.Exists(destFilePath))
                File.Delete(destFilePath);

            File.Move(FilePath, destFilePath);

            if (_backupCount > 0)
                foreach (string path in GetFilePathsToDelete())
                    File.Delete(path);

            _writer = Open(FilePath, _encoding);

            DateTime newRollOver = RollOverAt;
            DateTime cTime = CurrentTime;
            while (newRollOver <= cTime)
                newRollOver += _when;

            RollOverAt = newRollOver;
        }
    }
}
