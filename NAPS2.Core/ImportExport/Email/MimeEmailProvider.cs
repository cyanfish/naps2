using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email
{
    public abstract class MimeEmailProvider : IEmailProvider
    {
        public async Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var builder = new BodyBuilder
            {
                // Ensure there is some content (a newline is fine) to work around the buggy new gmail UI
                TextBody = string.IsNullOrWhiteSpace(emailMessage.BodyText) ? "\n" : emailMessage.BodyText
            };
            foreach (var attachment in emailMessage.Attachments)
            {
                builder.Attachments.Add(attachment.FilePath);
            }

            var message = new MimeMessage();
            CopyRecips(emailMessage.Recipients, EmailRecipientType.To, message.To);
            CopyRecips(emailMessage.Recipients, EmailRecipientType.Cc, message.Cc);
            CopyRecips(emailMessage.Recipients, EmailRecipientType.Bcc, message.Bcc);
            message.Subject = emailMessage.Subject ?? "";
            message.Body = builder.ToMessageBody();

            await SendMimeMessage(message, progressCallback, cancelToken, emailMessage.AutoSend);

            return true;
        }

        protected abstract Task SendMimeMessage(MimeMessage message, ProgressHandler progressCallback, CancellationToken cancelToken, bool autoSend);

        private void CopyRecips(List<EmailRecipient> recips, EmailRecipientType type, InternetAddressList outputList)
        {
            foreach (var recip in recips.Where(x => x.Type == type))
            {
                outputList.Add(new MailboxAddress(Encoding.UTF8, recip.Name, recip.Address));
            }
        }
    }
}
