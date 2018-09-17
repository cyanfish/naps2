using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NAPS2.Util;

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
                if (!commandText.StartsWith("\"", StringComparison.InvariantCulture))
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
            var dllPath = clientName == null ? null : GetDllPath(clientName);
            if (dllPath == null)
            {
                dllPath = "mapi32.dll";
            }
            var module = Win32.LoadLibrary(dllPath);
            var addr = Win32.GetProcAddress(module, "MAPISendMailW");
            if (addr == IntPtr.Zero)
            {
                addr = Win32.GetProcAddress(module, "MAPISendMailHelper");
            }
            return (MapiSendMailDelegate)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegate));
        }

        private static string GetDllPath(string clientName)
        {
            using (var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}"))
            {
                return clientKey?.GetValue("DllPathEx")?.ToString() ?? clientKey?.GetValue("DllPath")?.ToString();
            }
        }

        // MAPISendMail is documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
        internal delegate MapiSendMailReturnCode MapiSendMailDelegate(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);
    }
}
