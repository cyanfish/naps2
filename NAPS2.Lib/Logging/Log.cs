using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Logging;

/// <summary>
/// Logging functionality.
/// </summary>
public static class Log
{
    private static ILogger _logger = NullLogger.Instance;
    private static IEventLogger _eventLogger = new NullEventLogger();

    public static ILogger Logger
    {
        get => _logger;
        set => _logger = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IEventLogger EventLogger
    {
        get => _eventLogger;
        set => _eventLogger = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static void Info(string message, params object[] args)
    {
        _logger.LogInformation(string.Format(message, args));
    }

    public static void Error(string message, params object[] args)
    {
        _logger.LogError(string.Format(message, args));
    }

    public static void ErrorException(string message, Exception exception)
    {
        _logger.LogError(exception, message);
    }

    public static void FatalException(string message, Exception exception)
    {
        _logger.LogCritical(exception, message);
    }

    public static void Event(EventType eventType, EventParams eventParams)
    {
        _eventLogger.LogEvent(eventType, eventParams);
    }
}