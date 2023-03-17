using System.Diagnostics;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    public class DebugHandler: Handler, IHandler
    {
        public DebugHandler(IFormatter? format = null) : base()
        {
            Formatter = format ?? Defaults.Formatter;
        }

        protected override void Emit(LogRecord record)
        {
            string message = Format(record);
            Debug.WriteLine(message);
        }

        public override void Flush()
        {

        }

        public override void Close()
        {

        }
    }
}