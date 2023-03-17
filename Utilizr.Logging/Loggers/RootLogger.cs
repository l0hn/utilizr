namespace Utilizr.Logging.Loggers
{
    public class RootLogger : Logger
    {
        internal RootLogger(LoggingLevel level) : base("root", level)
        {
            Root = this;
        }
    }
}
