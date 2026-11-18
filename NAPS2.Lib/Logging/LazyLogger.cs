using Microsoft.Extensions.Logging;

namespace NAPS2.Logging;

/// <summary>
/// Defers creating the inner logger until a message is logged at an enabled level, as initializing NLog can
/// noticeably slow down startup.
/// </summary>
public class LazyLogger : ILogger
{
    private readonly Lazy<ILogger> _inner;
    private readonly Func<LogLevel, bool> _isEnabled;

    public LazyLogger(Func<ILogger> inner, Func<LogLevel, bool> isEnabled)
    {
        _inner = new Lazy<ILogger>(inner);
        _isEnabled = isEnabled;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            _inner.Value.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && _isEnabled(logLevel);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _inner.Value.BeginScope(state);
    }
}
