using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using NAPS2.Config;

namespace NAPS2.Util
{
    /// <summary>
    /// Helps manage the lifecycle of the NAPS2 GUI.
    /// </summary>
    public class Lifecycle
    {
        private const string STI_DEVICE_PREFIX = "/StiDevice:";
        private const string STI_EVENT_PREFIX = "/StiEvent:";

        private readonly StillImage sti;
        private readonly AppConfigManager appConfigManager;
        private readonly WindowsEventLogger eventLogger;

        private bool registeredSti;
        private bool createdEventSource;

        public Lifecycle(StillImage sti, AppConfigManager appConfigManager, WindowsEventLogger eventLogger)
        {
            this.sti = sti;
            this.appConfigManager = appConfigManager;
            this.eventLogger = eventLogger;
        }

        /// <summary>
        /// Parses the NAPS2 GUI command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public void ParseArgs(string[] args)
        {
            bool silent = args.Any(x => x.Equals("/Silent", StringComparison.InvariantCultureIgnoreCase));
            bool noElevation = args.Any(x => x.Equals("/NoElevation", StringComparison.InvariantCultureIgnoreCase));

            foreach (var arg in args)
            {
                try
                {
                    if (arg.StartsWith(STI_DEVICE_PREFIX))
                    {
                        sti.DeviceID = arg.Substring(STI_DEVICE_PREFIX.Length);
                        sti.DoScan = true;
                    }
                    else if (arg.Equals("/RegisterSti", StringComparison.InvariantCultureIgnoreCase))
                    {
                        registeredSti = true;
                        sti.RegisterSti(silent);
                    }
                    else if (arg.Equals("/UnregisterSti", StringComparison.InvariantCultureIgnoreCase))
                    {
                        registeredSti = true;
                        sti.UnregisterSti(silent);
                    }
                    else if (arg.Equals("/CreateEventSource", StringComparison.InvariantCultureIgnoreCase))
                    {
                        createdEventSource = true;
                        eventLogger.CreateEventSource(silent);
                    }
                }
                catch (Exception ex)
                {
                    if (!noElevation && !IsElevated)
                    {
                        RelaunchAsElevated();
                        return;
                    }

                    Log.ErrorException($"Error running {arg}", ex);
                    if (!silent)
                    {
                        MessageBox.Show($@"Error running {arg}. Maybe run as administrator?");
                    }
                }
            }
        }

        public bool IsElevated
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                if (identity == null)
                {
                    return false;
                }
                var pricipal = new WindowsPrincipal(identity);
                return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void RelaunchAsElevated()
        {
            Process.Start(new ProcessStartInfo
            {
                Verb = "runas",
                FileName = Assembly.GetEntryAssembly().Location,
                Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1)) + " /NoElevation"
            });
        }

        /// <summary>
        /// May terminate the NAPS2 GUI based on the command-line arguments and running processes, sending messages to other processes if appropriate.
        /// </summary>
        public void ExitIfRedundant()
        {
            if (registeredSti)
            {
                // Was just started by the user to (un)register STI
                Environment.Exit(sti.RegisterOk ? 0 : 1);
            }
            if (createdEventSource)
            {
                Environment.Exit(0);
            }

            // If this instance of NAPS2 was spawned by STI, then there may be another instance of NAPS2 we want to get the scan signal instead
            if (sti.DoScan)
            {
                // Try each possible process in turn until one receives the message (most recently started first)
                foreach (var process in GetOtherNaps2Processes())
                {
                    // Another instance of NAPS2 is running, so send it the "Scan" signal
                    ActivateProcess(process);
                    if (Pipes.SendMessage(process, Pipes.MSG_SCAN_WITH_DEVICE + sti.DeviceID))
                    {
                        // Successful, so this instance can be closed before showing any UI
                        Environment.Exit(0);
                    }
                }
            }

            // Only start one instance if configured for SingleInstance
            if (appConfigManager.Config.SingleInstance)
            {
                // See if there's another NAPS2 process running
                foreach (var process in GetOtherNaps2Processes())
                {
                    // Another instance of NAPS2 is running, so send it the "Activate" signal
                    ActivateProcess(process);
                    if (Pipes.SendMessage(process, Pipes.MSG_ACTIVATE))
                    {
                        // Successful, so this instance should be closed
                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void ActivateProcess(Process process)
        {
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                Win32.SetForegroundWindow(process.MainWindowHandle);
            }
        }

        private static IEnumerable<Process> GetOtherNaps2Processes()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var otherProcesses = Process.GetProcessesByName(currentProcess.ProcessName)
                .Where(x => x.Id != currentProcess.Id)
                .OrderByDescending(x => x.StartTime);
            return otherProcesses;
        }
    }
}
