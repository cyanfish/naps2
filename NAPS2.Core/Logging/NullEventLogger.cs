using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Logging
{
    public class NullEventLogger : IEventLogger
    {
        public void LogEvent(EventType eventType, Event evt)
        {
        }
    }
}
