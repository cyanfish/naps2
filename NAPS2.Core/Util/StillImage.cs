using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

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

        private const string REGKEY_AUTOPLAY_HANDLER_NAPS2 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{1c3a7177-f3a7-439e-be47-e304a185f932}";
        private const string REGKEY_STI_APP = @"SOFTWARE\Microsoft\Windows\CurrentVersion\StillImage\Registered Applications";
        private const string REGKEY_STI_EVENT_NAPS2 = @"SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{1c3a7177-f3a7-439e-be47-e304a185f932}";

        private bool registered;
        private bool registerOk;

        public void ParseArgs(string[] args)
        {
            bool silent = args.Any(x => x.Equals("/silent", StringComparison.InvariantCultureIgnoreCase));
            foreach (var arg in args)
            {
                if (arg.StartsWith(DEVICE_PREFIX))
                {
                    DeviceID = arg.Substring(DEVICE_PREFIX.Length);
                    DoScan = true;
                }
                else if (arg.Equals("/registersti", StringComparison.InvariantCultureIgnoreCase))
                {
                    RegisterSti(silent);
                    registered = true;
                }
                else if (arg.Equals("/unregistersti", StringComparison.InvariantCultureIgnoreCase))
                {
                    UnregisterSti(silent);
                    registered = true;
                }
            }
        }

        private void RegisterSti(bool silent)
        {
            try
            {
                var exe = Assembly.GetEntryAssembly().Location;

                using (var key1 = Registry.LocalMachine.CreateSubKey(REGKEY_AUTOPLAY_HANDLER_NAPS2))
                {
                    key1.SetValue("Action", "Scan with NAPS2");
                    key1.SetValue("CLSID", "WIACLSID");
                    key1.SetValue("DefaultIcon", "sti.dll,0");
                    key1.SetValue("InitCmdLine", string.Format("/WiaCmd;{0} /StiDevice:%1 /StiEvent:%2;", exe));
                    key1.SetValue("Provider", "NAPS2");
                }

                using (var key2 = Registry.LocalMachine.CreateSubKey(REGKEY_STI_APP))
                {
                    key2.SetValue("NAPS2", exe);
                }

                using (var key3 = Registry.LocalMachine.CreateSubKey(REGKEY_STI_EVENT_NAPS2))
                {
                    key3.SetValue("Cmdline", string.Format("{0} /StiDevice:%1 /StiEvent:%2", exe));
                    key3.SetValue("Desc", "Scan with NAPS2");
                    key3.SetValue("Icon", string.Format("{0},0", exe));
                    key3.SetValue("Name", "NAPS2");
                }

                registerOk = true;
                if (!silent)
                {
                    MessageBox.Show(@"Successfully registered STI.");
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error registering STI", ex);
                if (!silent)
                {
                    MessageBox.Show(@"Error registering STI. Maybe run as administrator?");
                }
            }
        }

        private void UnregisterSti(bool silent)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKey(REGKEY_AUTOPLAY_HANDLER_NAPS2, false);
                using (var key2 = Registry.LocalMachine.OpenSubKey(REGKEY_STI_APP, true))
                {
                    if (key2 != null)
                    {
                        key2.DeleteValue("NAPS2", false);
                    }
                }
                Registry.LocalMachine.DeleteSubKey(REGKEY_STI_EVENT_NAPS2, false);

                registerOk = true;
                if (!silent)
                {
                    MessageBox.Show(@"Successfully unregistered STI.");
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error unregistering STI", ex);
                if (!silent)
                {
                    MessageBox.Show(@"Error unregistering STI. Maybe run as administrator?");
                }
            }
        }

        public void ExitIfRedundant()
        {
            if (registered)
            {
                // Was just started by the user to (un)register STI
                Environment.Exit(registerOk ? 0 : 1);
            }
            // If this instance of NAPS2 was spawned by STI, then there may be another instance of NAPS2 we want to get the scan signal instead
            if (DoScan)
            {
                Process current = Process.GetCurrentProcess();
                // Try each possible process in turn until one receives the message (most recently started first)
                foreach (var process in Process.GetProcessesByName(current.ProcessName).Where(x => x.Id != current.Id).OrderByDescending(x => x.StartTime))
                {
                    // Another instance of NAPS2 is running, so send it the "Scan" signal
                    if (Pipes.SendMessage(process, Pipes.MSG_SCAN_WITH_DEVICE + DeviceID))
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
