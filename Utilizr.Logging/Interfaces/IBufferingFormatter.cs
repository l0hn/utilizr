namespace Utilizr.Logging.Interfaces
{
    public interface IBufferingFormatter
    {
        string Format(LogRecord[] records);
    }
}
