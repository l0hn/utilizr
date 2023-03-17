using System;
using System.Text;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Handlers
{
    public abstract class BaseRotatingHandler : FileHandler, IHandler
    {
        protected int _backupCount;

        public BaseRotatingHandler(string filePath, bool append = true, bool delay = false)
            : base(filePath, append, delay)
        {
        }

        public BaseRotatingHandler(string filePath, Encoding? encoding, bool append = true, bool delay = false)
            : base(filePath, encoding, append, delay)
        {
        }

        protected abstract bool ShouldRollover(LogRecord record);

        protected abstract void DoRollover();

        protected override void Emit(LogRecord record)
        {
            if (ShouldRollover(record))
                DoRollover();

            base.Emit(record);
        }
    }
}
