
using System;
using Microsoft.Extensions.Logging;

namespace Utilizr.Logging;

public class ILoggerWrapper : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logging.Log.Message(GetLogLevel(logLevel), formatter(state, exception));
    }


    LoggingLevel GetLogLevel(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                return LoggingLevel.DEBUG;
            case LogLevel.Information:
                return LoggingLevel.INFO;
            case LogLevel.Warning:
                return LoggingLevel.WARNING;
            case LogLevel.Error:
                return LoggingLevel.ERROR;
            case LogLevel.Critical:
                return LoggingLevel.CRITICAL;
            case LogLevel.None:
            default:
                return LoggingLevel.INFO;
        }
    }

}

public class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();

    private NullScope() { }
    public void Dispose()
    {
    }
}
