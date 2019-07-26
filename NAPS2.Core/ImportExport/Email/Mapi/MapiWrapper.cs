using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class MapiWrapper
    {
        private readonly SystemEmailClients systemEmailClients;

        public MapiWrapper(SystemEmailClients systemEmailClients)
        {
            this.systemEmailClients = systemEmailClients;
        }

        public bool CanLoadClient(string clientName) => systemEmailClients.GetLibrary(clientName) != IntPtr.Zero;

        public MapiSendMailReturnCode SendEmail(string clientName, EmailMessage message)
        {
            var (mapiSendMail, mapiSendMailW) = systemEmailClients.GetDelegate(clientName, out bool unicode);

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
        }

        private static MapiSendMailReturnCode SendMail(SystemEmailClients.MapiSendMailDelegate mapiSendMail, EmailMessage message, MapiSendMailFlags flags)
        {
            using (var files = Unmanaged.CopyOf(GetFiles(message)))
            using (var recips = Unmanaged.CopyOf(GetRecips(message)))
            {
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
        }

        private static MapiSendMailReturnCode SendMailW(SystemEmailClients.MapiSendMailDelegateW mapiSendMailW, EmailMessage message, MapiSendMailFlags flags)
        {
            using (var files = Unmanaged.CopyOf(GetFilesW(message)))
            using (var recips = Unmanaged.CopyOf(GetRecipsW(message)))
            {
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
}
