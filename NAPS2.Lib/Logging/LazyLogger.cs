using Microsoft.Extensions.Logging;

namespace NAPS2.Logging;

public class LazyLogger : ILogger
{
    private readonly Lazy<ILogger> _inner;

    public LazyLogger(Func<ILogger> inner)
    {
        _inner = new Lazy<ILogger>(inner);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _inner.Value.Log(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _inner.Value.IsEnabled(logLevel);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _inner.Value.BeginScope(state);
    }
}