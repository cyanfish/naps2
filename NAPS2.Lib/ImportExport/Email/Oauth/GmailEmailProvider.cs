using MimeKit;

namespace NAPS2.ImportExport.Email.Oauth;

internal class GmailEmailProvider : MimeEmailProvider
{
    private readonly GmailOauthProvider _gmailOauthProvider;

    public GmailEmailProvider(GmailOauthProvider gmailOauthProvider)
    {
        _gmailOauthProvider = gmailOauthProvider;
    }

    protected override async Task SendMimeMessage(MimeMessage message, ProgressHandler progress, bool autoSend)
    {
        var draft = await _gmailOauthProvider.UploadDraft(message.ToString(), progress);
        if (autoSend)
        {
            await _gmailOauthProvider.SendDraft(draft.DraftId);
        }
        else
        {
            var userEmail = _gmailOauthProvider.User;
            // Open the draft in the user's browser
            ProcessHelper.OpenUrl(
                $"https://mail.google.com/mail/?authuser={userEmail}#drafts?compose={draft.MessageId}");
        }
    }

    public override bool IsAvailable => _gmailOauthProvider.HasClientCreds;
}