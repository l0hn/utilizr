namespace Utilizr.Logging.Interfaces
{
    public interface IFormatter
    {
        string Format(LogRecord record);
    }
}
