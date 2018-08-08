using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class SystemEmailClients
    {
        public string GetDefaultName()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Clients\Mail", false))
            {
                return key?.GetValue(null).ToString();
            }
        }

        public string[] GetNames()
        {
            // TODO: Swallow errors
            using (var clientList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\Mail", false))
            {
                return clientList?.GetSubKeyNames().Where(clientName =>
                {
                    using (var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}"))
                    {
                        return clientKey?.GetValue("DllPath") != null;
                    }
                }).ToArray() ?? new string[0];
            }
        }

        public Image GetIcon(string clientName)
        {
            using (var command = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}\shell\open\command", false))
            {
                string commandText = command?.GetValue(null).ToString() ?? "";
                if (!commandText.StartsWith("\""))
                {
                    return null;
                }
                string exePath = commandText.Substring(1, commandText.IndexOf("\"", 1, StringComparison.InvariantCulture) - 1);
                var icon = Icon.ExtractAssociatedIcon(exePath);
                return icon?.ToBitmap();
            }
        }

        internal MapiSendMailDelegate GetDelegate(string clientName)
        {
            if (clientName == null)
            {
                return MAPISendMail;
            }
            var dllPath = GetDllPath(clientName);
            if (dllPath == null)
            {
                return MAPISendMail;
            }
            var module = LoadLibrary(dllPath);
            var addr = GetProcAddress(module, "MAPISendMail");
            return (MapiSendMailDelegate)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegate));
        }

        private static string GetDllPath(string clientName)
        {
            using (var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}"))
            {
                return clientKey?.GetValue("DllPathEx")?.ToString() ?? clientKey?.GetValue("DllPath")?.ToString();
            }
        }

        internal delegate MapiSendMailReturnCode MapiSendMailDelegate(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);

        // MAPISendMail is documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx

        [DllImport("mapi32.DLL")]
        private static extern MapiSendMailReturnCode MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string procName);

    }
}
