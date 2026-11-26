using Microsoft.Extensions.Logging;

namespace NAPS2.Logging;

/// <summary>
/// Forwards trace output (e.g. from NTwain) to an ILogger.
/// </summary>
public class LoggerTraceListener(ILogger logger) : TraceListener
{
    public override void Write(string? message) => WriteLine(message);

    public override void WriteLine(string? message) => logger.LogDebug("{Message}", message);

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id,
        string? message)
    {
        var level = eventType switch
        {
            TraceEventType.Critical => LogLevel.Critical,
            TraceEventType.Error => LogLevel.Error,
            TraceEventType.Warning => LogLevel.Warning,
            TraceEventType.Information => LogLevel.Information,
            _ => LogLevel.Debug
        };
        logger.Log(level, "{Source}: {Message}", source, message);
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id,
        string? format, params object?[]? args)
    {
        TraceEvent(eventCache, source, eventType, id,
            format == null || args == null ? format : string.Format(format, args));
    }
}
