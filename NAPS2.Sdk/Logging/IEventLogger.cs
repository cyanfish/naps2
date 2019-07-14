namespace NAPS2.Logging
{
    public interface IEventLogger
    {
        void LogEvent(EventType eventType, EventParams eventParams);
    }
}
