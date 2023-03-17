using System;

namespace Utilizr.Logging.Interfaces
{
    public interface IHandler: IDisposable
    {
        LoggingLevel Level { get; set; }
        IFormatter? Formatter { get; set; }
        bool FilterRecord(LogRecord record);
        bool Handle(LogRecord record);
        void Flush();
        void Close();
    }
}
