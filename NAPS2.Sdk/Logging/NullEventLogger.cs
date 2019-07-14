namespace NAPS2.Logging
{
    public class NullEventLogger : IEventLogger
    {
        public void LogEvent(EventType eventType, EventParams eventParams)
        {
        }
    }
}
