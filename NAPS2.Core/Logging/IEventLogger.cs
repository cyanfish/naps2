using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Logging
{
    public interface IEventLogger
    {
        void LogEvent(EventType eventType, Event evt);
    }
}
