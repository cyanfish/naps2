using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth;

internal class OutlookWebEmailProvider : IEmailProvider
{
    private readonly OutlookWebOauthProvider _outlookWebOauthProvider;

    public OutlookWebEmailProvider(OutlookWebOauthProvider outlookWebOauthProvider)
    {
        _outlookWebOauthProvider = outlookWebOauthProvider;
    }

    public async Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default)
    {
        var messageObj = new JObject
        {
            ["subject"] = emailMessage.Subject,
            ["body"] = new JObject
            {
                ["contentType"] = "text",
                ["content"] = emailMessage.BodyText
            },
            ["toRecipients"] = Recips(emailMessage, EmailRecipientType.To),
            ["ccRecipients"] = Recips(emailMessage, EmailRecipientType.Cc),
            ["bccRecipients"] = Recips(emailMessage, EmailRecipientType.Bcc),
            ["attachments"] = new JArray(emailMessage.Attachments.Select(attachment => new JObject
            {
                ["@odata.type"] = "#microsoft.graph.fileAttachment",
                ["name"] = attachment.AttachmentName,
                ["contentBytes"] = Convert.ToBase64String(File.ReadAllBytes(attachment.FilePath))
            }))
        };
        var draft = await _outlookWebOauthProvider.UploadDraft(messageObj.ToString(), progress);


        if (emailMessage.AutoSend)
        {
            await _outlookWebOauthProvider.SendDraft(draft.MessageId);
        }
        else
        {
            // Open the draft in the user's browser
            ProcessHelper.OpenUrl(draft.WebLink + "&ispopout=0");
        }

        return true;
    }

    private JToken Recips(EmailMessage message, EmailRecipientType type)
    {
        return new JArray(message.Recipients.Where(recip => recip.Type == type).Select(recip => new JObject
        {
            {
                "emailAddress", new JObject
                {
                    { "address", recip.Address },
                    { "name", recip.Name }
                }
            }
        }));
    }
}