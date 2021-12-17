using System;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Unmanaged;

namespace NAPS2.ImportExport.Email.Mapi;

public class MapiWrapper : IMapiWrapper
{
    private readonly SystemEmailClients _systemEmailClients;
    private readonly IConfigProvider<EmailSetup> _emailSetupProvider;

    public MapiWrapper(SystemEmailClients systemEmailClients, IConfigProvider<EmailSetup> emailSetupProvider)
    {
        _systemEmailClients = systemEmailClients;
        _emailSetupProvider = emailSetupProvider;
    }

    public bool CanLoadClient
    {
        get
        {
            var clientName = _emailSetupProvider.Get(c => c.SystemProviderName);
            return _systemEmailClients.GetLibrary(clientName) != IntPtr.Zero;
        }
    }

    public Task<MapiSendMailReturnCode> SendEmail(EmailMessage message)
    {
        return Task.Run(() =>
        {
            var clientName = _emailSetupProvider.Get(c => c.SystemProviderName);
            var (mapiSendMail, mapiSendMailW) = _systemEmailClients.GetDelegate(clientName, out bool unicode);

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

            return unicode ? SendMailW(mapiSendMailW, message, flags) : SendMail(mapiSendMail, message, flags);
        });
    }

    private static MapiSendMailReturnCode SendMail(SystemEmailClients.MapiSendMailDelegate mapiSendMail, EmailMessage message, MapiSendMailFlags flags)
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

    private static MapiSendMailReturnCode SendMailW(SystemEmailClients.MapiSendMailDelegateW mapiSendMailW, EmailMessage message, MapiSendMailFlags flags)
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
}