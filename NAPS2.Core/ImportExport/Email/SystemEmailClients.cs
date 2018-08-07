using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace NAPS2.ImportExport.Email
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
    }
}
