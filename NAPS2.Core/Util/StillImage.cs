using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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
        private const string REGKEY_IMAGE_EVENTS = @"SYSTEM\CurrentControlSet\Control\Class\{6bdd1fc6-810f-11d0-bec7-08002be2092f}\0000\Events";

        public void ParseArgs(string[] args)
        {
            bool silent = args.Any(x => x.Equals("/Silent", StringComparison.InvariantCultureIgnoreCase));
            bool noElevation = args.Any(x => x.Equals("/NoElevation", StringComparison.InvariantCultureIgnoreCase));
            foreach (var arg in args)
            {
                if (arg.StartsWith(DEVICE_PREFIX))
                {
                    DeviceID = arg.Substring(DEVICE_PREFIX.Length);
                    DoScan = true;
                }
                else if (arg.Equals("/RegisterSti", StringComparison.InvariantCultureIgnoreCase))
                {
                    RegisterSti(silent, noElevation);
                    Registered = true;
                }
                else if (arg.Equals("/UnregisterSti", StringComparison.InvariantCultureIgnoreCase))
                {
                    UnregisterSti(silent, noElevation);
                    Registered = true;
                }
            }
        }

        private void RegisterSti(bool silent, bool noElevation)
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

                RegisterOk = true;
                if (!silent)
                {
                    MessageBox.Show(@"Successfully registered STI. A reboot may be needed.");
                }
            }
            catch (Exception ex)
            {
                if (!noElevation && !IsElevated)
                {
                    RelaunchAsElevated();
                    return;
                }

                Log.ErrorException("Error registering STI", ex);
                if (!silent)
                {
                    MessageBox.Show(@"Error registering STI. Maybe run as administrator?");
                }
            }
        }

        private void UnregisterSti(bool silent, bool noElevation)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKey(REGKEY_AUTOPLAY_HANDLER_NAPS2, false);
                using (var key2 = Registry.LocalMachine.OpenSubKey(REGKEY_STI_APP, true))
                {
                    key2?.DeleteValue("NAPS2", false);
                }
                Registry.LocalMachine.DeleteSubKey(REGKEY_STI_EVENT_NAPS2, false);

                var events = Registry.LocalMachine.OpenSubKey(REGKEY_IMAGE_EVENTS, true);
                if (events != null)
                {
                    foreach (var eventType in events.GetSubKeyNames())
                    {
                        events.DeleteSubKey(eventType + @"\{1C3A7177-F3A7-439E-BE47-E304A185F932}", false);
                    }
                }

                RegisterOk = true;
                if (!silent)
                {
                    MessageBox.Show(@"Successfully unregistered STI. A reboot may be needed.");
                }
            }
            catch (Exception ex)
            {
                if (!noElevation && !IsElevated)
                {
                    RelaunchAsElevated();
                    return;
                }

                Log.ErrorException("Error unregistering STI", ex);
                if (!silent)
                {
                    MessageBox.Show(@"Error unregistering STI. Maybe run as administrator?");
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

        public string DeviceID { get; private set; }

        public bool DoScan { get; private set; }

        public bool Registered { get; set; }

        public bool RegisterOk { get; set; }
    }
}
