using System.Drawing;
using Microsoft.Win32;
using NAPS2.Images.Gdi;

namespace NAPS2.ImportExport.Email.Mapi;

internal class MapiEmailClients : ISystemEmailClients
{
    public string? GetDefaultName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\Mail", false);
        return key?.GetValue(null)?.ToString();
    }

    public string[] GetNames()
    {
        // TODO: Swallow errors
        using var clientList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\Mail", false);
        return clientList?.GetSubKeyNames().Where(clientName =>
        {
            using var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}");
            return clientKey?.GetValue("DllPath") != null;
        }).ToArray() ?? Array.Empty<string>();
    }

    public IMemoryImage? LoadIcon(string clientName)
    {
        var exePath = GetExePath(clientName);
        if (exePath == null) return null;
        var icon = Icon.ExtractAssociatedIcon(exePath);
        if (icon == null) return null;
        return new GdiImage(icon.ToBitmap());
    }

    private string? GetExePath(string clientName)
    {
        using var command =
            Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}\shell\open\command", false);
        string commandText = command?.GetValue(null)?.ToString() ?? "";
        if (!commandText.StartsWith("\"", StringComparison.InvariantCulture))
        {
            return null;
        }
        return commandText.Substring(1, commandText.IndexOf("\"", 1, StringComparison.InvariantCulture) - 1);
    }
}