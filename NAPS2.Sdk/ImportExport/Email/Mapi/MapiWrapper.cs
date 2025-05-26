using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NAPS2.Platform.Windows;
using NAPS2.Unmanaged;

namespace NAPS2.ImportExport.Email.Mapi;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
internal class MapiWrapper : IMapiWrapper
{
    private const string DEFAULT_MAPI_DLL = "mapi32.dll";

    private readonly ILogger _logger;

    public MapiWrapper(ILogger logger)
    {
        _logger = logger;
    }

    public bool CanLoadClient(string? clientName) => GetLibrary(clientName) != IntPtr.Zero;

    public Task<MapiSendMailReturnCode> SendEmail(string? clientName, EmailMessage message)
    {
        return Task.Run(() =>
        {
            var (mapiSendMail, mapiSendMailW) = GetDelegate(clientName, out bool unicode);

            // Determine the flags used to send the message
            var flags = MapiSendMailFlags.None;
            if (!message.AutoSend)
            {
                flags |= MapiSendMailFlags.Dialog;
            }

            if (!message.AutoSend || !message.SilentSend)
            {
                flags |= MapiSendMailFlags.LogonUI;
            }

            return Invoker.Current.InvokeGet(() =>
                unicode ? SendMailW(mapiSendMailW!, message, flags) : SendMail(mapiSendMail!, message, flags));
        });
    }

    private static MapiSendMailReturnCode SendMail(MapiSendMailDelegate mapiSendMail, EmailMessage message, MapiSendMailFlags flags)
    {
        using var files = UnmanagedTypes.CopyOf(GetFiles(message));
        using var recips = UnmanagedTypes.CopyOf(GetRecips(message));
        // Create a MAPI structure for the entirety of the message
        var mapiMessage = new MapiMessage
        {
            subject = message.Subject,
            noteText = message.BodyText,
            recips = recips,
            recipCount = recips.Length,
            files = files,
            fileCount = files.Length
        };

        // Send the message
        return mapiSendMail(IntPtr.Zero, IntPtr.Zero, mapiMessage, flags, 0);
    }

    private static MapiSendMailReturnCode SendMailW(MapiSendMailDelegateW mapiSendMailW, EmailMessage message, MapiSendMailFlags flags)
    {
        using var files = UnmanagedTypes.CopyOf(GetFilesW(message));
        using var recips = UnmanagedTypes.CopyOf(GetRecipsW(message));
        // Create a MAPI structure for the entirety of the message
        var mapiMessage = new MapiMessageW
        {
            subject = message.Subject,
            noteText = message.BodyText,
            recips = recips,
            recipCount = recips.Length,
            files = files,
            fileCount = files.Length
        };

        // Send the message
        return mapiSendMailW(IntPtr.Zero, IntPtr.Zero, mapiMessage, flags, 0);
    }

    private static MapiRecipDesc[] GetRecips(EmailMessage message)
    {
        return message.Recipients.Select(recipient => new MapiRecipDesc
        {
            name = recipient.Name,
            address = "SMTP:" + recipient.Address,
            recipClass = recipient.Type == EmailRecipientType.Cc ? MapiRecipClass.Cc
                : recipient.Type == EmailRecipientType.Bcc ? MapiRecipClass.Bcc
                : MapiRecipClass.To
        }).ToArray();
    }

    private static MapiRecipDescW[] GetRecipsW(EmailMessage message)
    {
        return message.Recipients.Select(recipient => new MapiRecipDescW
        {
            name = recipient.Name,
            address = "SMTP:" + recipient.Address,
            recipClass = recipient.Type == EmailRecipientType.Cc ? MapiRecipClass.Cc
                : recipient.Type == EmailRecipientType.Bcc ? MapiRecipClass.Bcc
                : MapiRecipClass.To
        }).ToArray();
    }

    private static MapiFileDesc[] GetFiles(EmailMessage message)
    {
        return message.Attachments.Select(attachment => new MapiFileDesc
        {
            position = -1,
            path = attachment.FilePath,
            name = attachment.AttachmentName
        }).ToArray();
    }

    private static MapiFileDescW[] GetFilesW(EmailMessage message)
    {
        return message.Attachments.Select(attachment => new MapiFileDescW
        {
            position = -1,
            path = attachment.FilePath,
            name = attachment.AttachmentName
        }).ToArray();
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

    private string? GetDefaultName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\Mail", false);
        return key?.GetValue(null)?.ToString();
    }

    // MAPISendMail is documented at:
    // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
    internal delegate MapiSendMailReturnCode MapiSendMailDelegate(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);
    internal delegate MapiSendMailReturnCode MapiSendMailDelegateW(IntPtr session, IntPtr hwnd, MapiMessageW message, MapiSendMailFlags flags, int reserved);
}