using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NAPS2.Util
{
    public class StillImage
    {
        private const string DEVICE_PREFIX = "/StiDevice:";
        private const string EVENT_PREFIX = "/StiEvent:";

        private const string GUID_DEVICE_ARRIVED_LAUNCH = "{740d9ee6-70f1-11d1-ad10-00a02438ad48}";
        private const string GUID_SCAN_IMAGE = "{a6c5a715-8c6e-11d2-977a-0000f87a926f}";
        private const string GUID_SCAN_PRINT_IMAGE = "{b441f425-8c6e-11d2-977a-0000f87a926f}";
        private const string GUID_SCAN_FAX_IMAGE = "{c00eb793-8c6e-11d2-977a-0000f87a926f}";

        public void ParseArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith(DEVICE_PREFIX))
                {
                    DeviceID = arg.Substring(DEVICE_PREFIX.Length);
                    DoScan = true;
                }
            }
        }

        public void ExitIfRedundant()
        {
            // If this instance of NAPS2 was spawned by STI, then there may be another instance of NAPS2 we want to get the scan signal instead
            if (DoScan)
            {
                Process current = Process.GetCurrentProcess();
                if (Process.GetProcessesByName(current.ProcessName).Any(process => process.Id != current.Id))
                {
                    // Another instance of NAPS2 is running, so send it the "Scan" signal
                    if (Pipes.SendMessage(Pipes.MSG_SCAN_WITH_DEVICE + DeviceID))
                    {
                        // Successful, so this instance can be closed before showing any UI
                        Environment.Exit(0);
                    }
                }
            }
        }

        public string DeviceID { get; private set; }

        public bool DoScan { get; private set; }
    }
}
