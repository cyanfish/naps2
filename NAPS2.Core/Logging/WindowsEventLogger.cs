using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Util;

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

        public void CreateEventSource(bool silent)
        {
            if (!EventLog.SourceExists(SOURCE_NAME))
            {
                EventLog.CreateEventSource(SOURCE_NAME, LOG_NAME);
                if (!silent)
                {
                    MessageBox.Show(@"Successfully created event source.");
                }
            }
            else
            {
                if (!silent)
                {
                    MessageBox.Show(@"Event source already exists.");
                }
            }
        }

        public void LogEvent(EventType eventType, EventParams eventParams)
        {
            // TODO: AppConfig check
            try
            {
                EventLog.WriteEntry(SOURCE_NAME, eventParams.ToString(), EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error writing to windows event log", ex);
            }
        }
    }
}
