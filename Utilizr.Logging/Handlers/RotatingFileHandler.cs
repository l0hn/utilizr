using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Logging.Filters;
//using Utilizr.Extensions;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// Handler for logging to a set of files, which switches from one file to the next when the current file reaches a certain size.
    /// </summary>
    public class RotatingFileHandler : BaseRotatingHandler
    {
        protected long _maxBytes;

        /// <summary>
        /// Open the specified file and use it as the stream for logging.
        /// 
        /// By default, the file grows indefinitely. You can specify particular values of maxBytes and backupCount to allow the file to
        /// rollover at a predetermined size.
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <param name="maxBytes">If maxBytes is zero, rollover never occurs.</param>
        /// <param name="backupCount">Upper limit of log rolled over backups, if the log rolls over again with backupCount amount of backups
        /// then the oldest is deleted.</param>
        /// <param name="append"></param>
        /// <param name="delay"></param>
        public RotatingFileHandler(string filePath, Encoding? encoding, long maxBytes = 0, int backupCount = 0, bool append = true, bool delay = false)
            : base(filePath, encoding, AdjustAppendParam(maxBytes, append), delay)
        {
            _maxBytes = maxBytes;
            _backupCount = backupCount;
        }

        public RotatingFileHandler(string filePath, long maxBytes = 0, int backupCount = 0, bool append = true, bool delay = false)
            : this(filePath, null, maxBytes, backupCount, append, delay)
        {
        }

        private static bool AdjustAppendParam(long maxBytes, bool append)
        {
            if (maxBytes > 0)
                return true;

            return append;
        }

        /// <summary>
        /// Roll over the log file.
        /// </summary>
        protected override void DoRollover()
        {
            _writer?.Close();
            _writer = null;

            static void replace(string source, string destination, string zipExt)
            {
                if (File.Exists(destination))
                    File.Delete(destination);

                // .log.zip renamed but old .log still exists. Delete both
                var noZipExt = destination.TrimEnd(zipExt);
                if (destination != noZipExt)
                {
                    if (File.Exists(noZipExt))
                        File.Delete(noZipExt);
                }

                File.Move(source, destination);
            };

            if (_backupCount > 0)
            {
                string basePath = Path.Combine(Path.GetDirectoryName(FilePath)!, Path.GetFileNameWithoutExtension(FilePath));
                string extension = Path.GetExtension(FilePath);
                const string zipExt = ".zip";
                string destPath;

                for (int i = _backupCount - 1; i > 0; i--)
                {
                    var sourcePath = $"{basePath}.{i}{extension}";
                    destPath = $"{basePath}.{i + 1}{extension}";
                    if (!File.Exists(sourcePath))
                    {
                        // previously archived so will have second extension of .zip
                        sourcePath = $"{sourcePath}{zipExt}";
                        destPath = $"{destPath}{zipExt}";
                    }

                    if (File.Exists(sourcePath))
                        replace(sourcePath, destPath, zipExt);
                }

                destPath = $"{basePath}.{1}{extension}";
                replace(FilePath, destPath, zipExt);
                ArchiveFile(destPath, zipExt);
            }
            _writer = Open(FilePath, _encoding);
        }

        static void ArchiveFile(string filePath, string zipExt)
        {
            Task.Run(() =>
            {
                try
                {
                    using (var zipFileSteam = new FileStream($"{filePath}{zipExt}", FileMode.Create, FileAccess.ReadWrite))
                    using (var zip = new ZipArchive(zipFileSteam, ZipArchiveMode.Create, true))
                    {
                        var archiveEntry = zip.CreateEntry(Path.GetFileName(filePath));
                        using var archiveStream = archiveEntry.Open();
                        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fs.CopyTo(archiveStream);
                    }

                    File.Delete(filePath);
                }
                catch (Exception)
                {

                }
            });
        }

        /// <summary>
        /// Determine if rollover should occur.
        /// 
        /// Basically, see if the supplied record would cause the file to exceed the size limit set.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected override bool ShouldRollover(LogRecord record)
        {
            _writer ??= Open(FilePath, _encoding);

            if (_maxBytes > 0)
            {
                string message = Format(record) + Environment.NewLine;
                //_writer.BaseStream.Seek(0, SeekOrigin.End);
                if (_writer.BaseStream.Length + message.Length >= _maxBytes)
                    return true;
            }
            return false;
        }
    }
}
