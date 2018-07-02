using NAPS2.Lang.Resources;
using NAPS2.Util;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Email.Mapi
{
    public class MapiEmailer : IEmailer
    {
        // MAPISendMail is documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
        // TODO: migrate to https://github.com/jstedfast/MailKit

        [DllImport("MAPI32.DLL")]
        private static extern MapiSendMailReturnCode MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, MapiSendMailFlags flags, int reserved);

        private readonly IErrorOutput errorOutput;

        public MapiEmailer(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
        }

        /// <summary>
        /// Sends an email described by the given message object.
        /// </summary>
        /// <param name="emailMessage">The object describing the email message.</param>
        /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
        public bool SendEmail(EmailMessage emailMessage)
        {
            // Translate files & recipients to unmanaged MAPI structures
            using (var files = Unmanaged.CopyOf(GetFiles(emailMessage)))
            using (var recips = Unmanaged.CopyOf(GetRecips(emailMessage)))
            {
                // Create a MAPI structure for the entirety of the message
                var mapiMessage = new MapiMessage
                {
                    subject = emailMessage.Subject,
                    noteText = emailMessage.BodyText,
                    recips = recips,
                    recipCount = recips.Length,
                    files = files,
                    fileCount = files.Length
                };

                // Determine the flags used to send the message
                var flags = MapiSendMailFlags.None;
                if (!emailMessage.AutoSend)
                {
                    flags |= MapiSendMailFlags.Dialog;
                }
                if (!emailMessage.AutoSend || !emailMessage.SilentSend)
                {
                    flags |= MapiSendMailFlags.LogonUI;
                }

                // Send the message
                var returnCode = MAPISendMail(IntPtr.Zero, IntPtr.Zero, mapiMessage, flags, 0);

                // Process the result
                if (returnCode == MapiSendMailReturnCode.UserAbort)
                {
                    return false;
                }
                if (returnCode != MapiSendMailReturnCode.Success)
                {
                    Log.Error("Error sending email. MAPI error code: {0}", returnCode);
                    errorOutput.DisplayError(MiscResources.EmailError, string.Format("MAPI returned error code: {0}", returnCode));
                    return false;
                }
                return true;
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

        private static MapiFileDesc[] GetFiles(EmailMessage message)
        {
            return message.Attachments.Select(attachment => new MapiFileDesc
            {
                position = -1,
                path = attachment.FilePath,
                name = attachment.AttachmentName
            }).ToArray();
        }
    }
}