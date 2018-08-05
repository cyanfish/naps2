using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;

namespace NAPS2.ImportExport.Email.Imap
{
    public abstract class ImapEmailProvider : IEmailProvider
    {
        public bool SendEmail(EmailMessage emailMessage)
        {
            var builder = new BodyBuilder
            {
                TextBody = emailMessage.BodyText
            };
            foreach (var attachment in emailMessage.Attachments)
            {
                builder.Attachments.Add(attachment.FilePath);
            }

            var message = new MimeMessage();
            CopyRecips(emailMessage.Recipients, EmailRecipientType.To, message.To);
            CopyRecips(emailMessage.Recipients, EmailRecipientType.Cc, message.Cc);
            CopyRecips(emailMessage.Recipients, EmailRecipientType.Bcc, message.Bcc);
            message.Subject = emailMessage.Subject;
            message.Body = builder.ToMessageBody();

            var client = new ImapClient();
            client.Connect(Host, 0, true);
            client.Authenticate(User, AccessToken);

            // TODO: Support auto-send
            var drafts = client.GetFolder(SpecialFolder.Drafts);
            var messageId = drafts.Append(message);
            if (!messageId.HasValue)
            {
                return false;
            }

            OpenDraft(drafts, messageId.Value);

            return true;
        }

        private void CopyRecips(List<EmailRecipient> recips, EmailRecipientType type, InternetAddressList outputList)
        {
            foreach (var recip in recips.Where(x => x.Type == type))
            {
                outputList.Add(new MailboxAddress(Encoding.UTF8, recip.Name, recip.Address));
            }
        }

        protected abstract string Host { get; }

        protected abstract string User { get; }

        protected abstract string AccessToken { get; }

        protected abstract void OpenDraft(IMailFolder drafts, UniqueId messageId);
    }
}
