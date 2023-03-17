using System;
using System.IO;
using System.Text;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// A handler class which writes formatted logging records to disk files.
    /// </summary>
    public class FileHandler : StreamHandler, IHandler
    {
        public string FilePath { get; private set; }

        private readonly bool _append;
        protected Encoding? _encoding;

        public FileHandler(string filePath, bool append = true, bool delay = false)
            : this (filePath, null, append, delay)
        {

        }

        public FileHandler(string filePath, Encoding? encoding, bool append = true, bool delay = false)
        {
            FilePath = filePath;
            _encoding = encoding;
            _append = append;
            if (!delay)
            {
                _writer = Open(filePath, encoding);
            }
        }

        public override void Close()
        {
            if (_writer == null)
                return;

            Flush();
            _writer.Close();
            base.Close();
            _writer = null;
        }

        /// <summary>
        /// Open the current base file with the original encoding (if specified).
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        protected StreamWriter Open(string filePath, Encoding? encoding = null)
        {
            FileMode fileMode = _append ? FileMode.Append : FileMode.Create;
            if (encoding == null)
                return new StreamWriter(new FileStream(filePath, fileMode, FileAccess.Write, FileShare.ReadWrite));

            return new StreamWriter(new FileStream(filePath, fileMode, FileAccess.Write, FileShare.ReadWrite), encoding);
        }

        protected override void Emit(LogRecord record)
        {
            _writer ??= Open(FilePath, _encoding);

            try
            {
                base.Emit(record);
            }
            catch (EncoderFallbackException)
            {
                try
                {
                    _writer.Dispose();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                _writer = null;
                throw;
            }
        }
    }
}
