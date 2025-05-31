using System.Text;
using MimeKit;

namespace NAPS2.ImportExport.Email;

internal abstract class MimeEmailProvider : IEmailProvider
{
    public async Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default)
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

        await SendMimeMessage(message, progress, emailMessage.AutoSend);

        return true;
    }

    protected abstract Task SendMimeMessage(MimeMessage message, ProgressHandler progress, bool autoSend);

    private void CopyRecips(List<EmailRecipient> recips, EmailRecipientType type, InternetAddressList outputList)
    {
        foreach (var recip in recips.Where(x => x.Type == type))
        {
            outputList.Add(new MailboxAddress(Encoding.UTF8, recip.Name, recip.Address));
        }
    }

    public abstract bool ShowInList { get; }

    public virtual bool CanSelectInList => true;
}