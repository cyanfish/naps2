namespace NAPS2.Logging;

// TODO: Use this through DI instead of Log.EventLogger for testability
public interface IEventLogger
{
    void LogEvent(EventType eventType, EventParams eventParams);
}