using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Logging
{
    public class WindowsEventLogger : IEventLogger
    {
        private const string SOURCE_NAME = "NAPS2";
        private const string LOG_NAME = "Application";

        private readonly AppConfigManager appConfigManager;

        public WindowsEventLogger(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
        }

        public void CreateEventSource()
        {
            if (!EventLog.SourceExists(SOURCE_NAME))
            {
                EventLog.CreateEventSource(SOURCE_NAME, LOG_NAME);
            }
        }

        public void LogEvent(EventType eventType, Event evt)
        {
            if ((eventType & appConfigManager.Config.EventLogging) != eventType) return;
            try
            {
                EventLog.WriteEntry(SOURCE_NAME, evt.ToString(), EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error writing to windows event log", ex);
            }
        }
    }
}
