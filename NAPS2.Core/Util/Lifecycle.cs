using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    /// <summary>
    /// Helps manage the lifecycle of the NAPS2 GUI.
    /// </summary>
    public class Lifecycle
    {
        private readonly StillImage sti;

        public Lifecycle(StillImage sti)
        {
            this.sti = sti;
        }

        /// <summary>
        /// Parses the NAPS2 GUI command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public void ParseArgs(string[] args)
        {
            sti.ParseArgs(args);
        }

        /// <summary>
        /// May terminate the NAPS2 GUI based on the command-line arguments and running processes, sending messages to other processes if appropriate.
        /// </summary>
        public void ExitIfRedundant()
        {
            if (sti.Registered)
            {
                // Was just started by the user to (un)register STI
                Environment.Exit(sti.RegisterOk ? 0 : 1);
            }
            // If this instance of NAPS2 was spawned by STI, then there may be another instance of NAPS2 we want to get the scan signal instead
            if (sti.DoScan)
            {
                Process current = Process.GetCurrentProcess();
                // Try each possible process in turn until one receives the message (most recently started first)
                foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(x => x.Id != current.Id).OrderByDescending(x => x.StartTime))
                {
                    // Another instance of NAPS2 is running, so send it the "Scan" signal
                    if (Pipes.SendMessage(process, Pipes.MSG_SCAN_WITH_DEVICE + sti.DeviceID))
                    {
                        // Successful, so this instance can be closed before showing any UI
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}
