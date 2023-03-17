using System;
using System.IO;
using System.Text;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    /// <summary>
    /// A handler class which writes logging records, appropriately formatted to a stream.
    /// Note that this class does not close the stream, as StandardError may be used.
    /// </summary>
    public class StreamHandler : Handler, IHandler
    {
        protected StreamWriter? _writer;

        /// <summary>
        /// </summary>
        /// <param name="stream">If stream is not specified, StandardError is used.</param>
        public StreamHandler(Stream? stream = null)
        {
            stream ??= Console.OpenStandardError();
            _writer = new StreamWriter(stream);
        }

        /// <summary>
        /// </summary>
        /// <param name="writer">writer to write the formatted log record events to</param>
        public StreamHandler(StreamWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public override void Flush()
        {
            _writer!.Flush();
        }

        protected override void Emit(LogRecord record)
        {
            var message = string.Empty;
            try
            {
                message = Format(record);
                _writer!.WriteLine(message);
                Flush();
            }
            catch (EncoderFallbackException)
            {
                Console.WriteLine($"LOGGER ENCODER EXCEPTION: [{message}]");
                throw;
            }
            catch(Exception ex)
            {
                // If same log name is being used by more than one instance, possible to write to the same
                // file at the same time, throwing System.IndexOutOfRangeException. Only real solution is
                // to open and close file stream on every entry, but performance implications. Just swallow
                // for the meantime and prevent bubbling up, crashing the app.

                Console.WriteLine($"{ex.GetType().Name}:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        public override void Close()
        {

        }

        public override void Dispose()
        {
            Close();
            _writer?.Dispose();
        }
    }
}