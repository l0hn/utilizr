namespace Utilizr.Logging.Interfaces
{
    public interface IFilter
    {
        bool FilterRecord(LogRecord record);
    }
}
