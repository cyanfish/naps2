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
        private const string DEFAULT_MAPI_DLL = "mapi32.dll";

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

        internal IntPtr GetLibrary(string clientName)
        {
            var dllPath = GetDllPath(clientName);
            return Win32.LoadLibrary(dllPath);
        }

        internal (MapiSendMailDelegate, MapiSendMailDelegateW) GetDelegate(string clientName, out bool unicode)
        {
            var dllPath = GetDllPath(clientName);
            var module = Win32.LoadLibrary(dllPath);
            if (module == IntPtr.Zero)
            {
                throw new Exception($"Could not load dll for email: {dllPath}");
            }
            var addr = Win32.GetProcAddress(module, "MAPISendMailW");
            if (addr != IntPtr.Zero)
            {
                unicode = true;
                return (null, (MapiSendMailDelegateW)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegateW)));
            }
            addr = Win32.GetProcAddress(module, "MAPISendMail");
            if (addr != IntPtr.Zero)
            {
                unicode = false;
                return ((MapiSendMailDelegate)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegate)), null);
            }
            throw new Exception($"Could not find an entry point in dll for email: {dllPath}");
        }

        private static string GetDllPath(string clientName)
        {
            if (clientName == null)
            {
                return DEFAULT_MAPI_DLL;
            }
            using (var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}"))
            {
                return clientKey?.GetValue("DllPathEx")?.ToString() ?? clientKey?.GetValue("DllPath")?.ToString() ?? DEFAULT_MAPI_DLL;
            }
        }

        // MAPISendMail is documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
        internal delegate MapiSendMailReturnCode MapiSendMailDelegate(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);
        internal delegate MapiSendMailReturnCode MapiSendMailDelegateW(IntPtr session, IntPtr hwnd, MapiMessageW message, MapiSendMailFlags flags, int reserved);
    }
}
