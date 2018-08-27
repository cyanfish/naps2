using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MimeKit;

namespace NAPS2.ImportExport.Email
{
    public abstract class MimeEmailProvider : IEmailProvider
    {
        public bool SendEmail(EmailMessage emailMessage)
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

            SendMimeMessage(message);

            return true;
        }

        protected abstract void SendMimeMessage(MimeMessage message);

        private void CopyRecips(List<EmailRecipient> recips, EmailRecipientType type, InternetAddressList outputList)
        {
            foreach (var recip in recips.Where(x => x.Type == type))
            {
                outputList.Add(new MailboxAddress(Encoding.UTF8, recip.Name, recip.Address));
            }
        }
    }
}
