using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NAPS2.Platform.Windows;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Email.Mapi;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
internal class SystemEmailClients
{
    private const string DEFAULT_MAPI_DLL = "mapi32.dll";

    private readonly ILogger _logger;

    public SystemEmailClients(ScanningContext scanningContext)
    {
        _logger = scanningContext.Logger;
    }

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

    public string? GetExePath(string clientName)
    {
        using var command = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}\shell\open\command", false);
        string commandText = command?.GetValue(null)?.ToString() ?? "";
        if (!commandText.StartsWith("\"", StringComparison.InvariantCulture))
        {
            return null;
        }
        return commandText.Substring(1, commandText.IndexOf("\"", 1, StringComparison.InvariantCulture) - 1);

    }

    internal IntPtr GetLibrary(string? clientName)
    {
        var dllPath = GetDllPath(clientName);
        return Win32.LoadLibrary(dllPath);
    }

    internal (MapiSendMailDelegate?, MapiSendMailDelegateW?) GetDelegate(string? clientName, out bool unicode)
    {
        _logger.LogDebug($"Using MAPI client {clientName ?? "<default>"}");
        var dllPath = GetDllPath(clientName);
        _logger.LogDebug($"Loading MAPI DLL {dllPath}");
        var module = Win32.LoadLibrary(dllPath);
        if (module == IntPtr.Zero)
        {
            throw new Exception($"Could not load dll for email: {dllPath}");
        }
        var addr = Win32.GetProcAddress(module, "MAPISendMailW");
        if (addr != IntPtr.Zero)
        {
            _logger.LogDebug("Using unicode function MAPISendMailW");
            unicode = true;
            return (null, (MapiSendMailDelegateW)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegateW)));
        }
        addr = Win32.GetProcAddress(module, "MAPISendMail");
        if (addr != IntPtr.Zero)
        {
            _logger.LogDebug("Using ansi function MAPISendMail");
            unicode = false;
            return ((MapiSendMailDelegate)Marshal.GetDelegateForFunctionPointer(addr, typeof(MapiSendMailDelegate)), null);
        }
        throw new Exception($"Could not find an entry point in dll for email: {dllPath}");
    }

    private string GetDllPath(string? clientName)
    {
        if (string.IsNullOrEmpty(clientName) || clientName == GetDefaultName())
        {
            return DEFAULT_MAPI_DLL;
        }
        using var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}");
        return clientKey?.GetValue("DllPathEx")?.ToString() ?? clientKey?.GetValue("DllPath")?.ToString() ?? DEFAULT_MAPI_DLL;
    }

    // MAPISendMail is documented at:
    // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
    internal delegate MapiSendMailReturnCode MapiSendMailDelegate(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);
    internal delegate MapiSendMailReturnCode MapiSendMailDelegateW(IntPtr session, IntPtr hwnd, MapiMessageW message, MapiSendMailFlags flags, int reserved);
}

