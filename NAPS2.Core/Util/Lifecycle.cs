using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Logging;

namespace NAPS2.Util
{
    /// <summary>
    /// A class to help manage the lifecycle of the NAPS2 GUI.
    /// </summary>
    public class Lifecycle
    {
        private readonly StillImage sti;
        private readonly AppConfigManager appConfigManager;
        private readonly WindowsEventLogger windowsEventLogger;

        private bool shouldCreateEventSource;
        private int returnCode;

        public Lifecycle(StillImage sti, AppConfigManager appConfigManager, WindowsEventLogger windowsEventLogger)
        {
            this.sti = sti;
            this.appConfigManager = appConfigManager;
            this.windowsEventLogger = windowsEventLogger;
        }

        /// <summary>
        /// Parses the NAPS2 GUI command-line arguments.
        /// </summary>
        /// <param name="args"></param>
        public void ParseArgs(string[] args)
        {
            bool silent = args.Any(x => x.Equals("/Silent", StringComparison.InvariantCultureIgnoreCase));
            bool noElevation = args.Any(x => x.Equals("/NoElevation", StringComparison.InvariantCultureIgnoreCase));

            // Utility function to send a message to the user (if /Silent is not specified)
            void Out(string message)
            {
                if (!silent)
                {
                    MessageBox.Show(message);
                }
            }

            // Utility function to run the given action, elevating to admin permissions if necessary (and /NoElevation is not specified)
            bool ElevationRequired(Action action)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception)
                {
                    if (!noElevation && !IsElevated)
                    {
                        RelaunchAsElevated();
                        return false;
                    }
                    throw;
                }
            }

            // Let StillImage figure out what it should do from the command-line args
            sti.ParseArgs(args);

            // Actually do any specified StillImage actions
            if (sti.ShouldRegister)
            {
                try
                {
                    if (ElevationRequired(sti.Register))
                    {
                        Out("Successfully registered STI. A reboot may be needed.");
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error registering STI", ex);
                    Out("Error registering STI. Maybe run as administrator?");
                    returnCode = 1;
                }
            }
            else if (sti.ShouldUnregister)
            {
                try
                {
                    if (ElevationRequired(sti.Unregister))
                    {
                        Out("Successfully unregistered STI. A reboot may be needed.");
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error unregistering STI", ex);
                    Out("Error unregistering STI. Maybe run as administrator?");
                    returnCode = 1;
                }
            }

            shouldCreateEventSource = args.Any(x => x.Equals("/CreateEventSource", StringComparison.InvariantCultureIgnoreCase));
            if (shouldCreateEventSource)
            {
                try
                {
                    if (ElevationRequired(windowsEventLogger.CreateEventSource))
                    {
                        Out("Successfully created event source.");
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error creating event source", ex);
                    Out("Error creating event source. Maybe run as administrator?");
                    returnCode = 1;
                }
            }
        }

        private bool IsElevated
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
            if (sti.ShouldRegister || sti.ShouldUnregister || shouldCreateEventSource)
            {
                // Was just started by the user to (un)register STI
                Environment.Exit(returnCode);
            }

            // If this instance of NAPS2 was spawned by STI, then there may be another instance of NAPS2 we want to get the scan signal instead
            if (sti.ShouldScan)
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
