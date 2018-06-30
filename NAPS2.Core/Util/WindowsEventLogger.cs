using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public class WindowsEventLogger
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
                    MessageBox.Show("Successfully created event source.");
                }
            }
            else
            {
                if (!silent)
                {
                    MessageBox.Show("Event source already exists.");
                }
            }
        }

        public void ScanComplete(ScanProfile profile, int pages)
        {
            // TODO: AppConfig check
            try
            {
                EventLog.WriteEntry(SOURCE_NAME, MiscResources.EventScanComplete, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error writing to windows event log", ex);
            }
        }
    }
}