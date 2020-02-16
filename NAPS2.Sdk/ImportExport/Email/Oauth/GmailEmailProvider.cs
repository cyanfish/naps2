using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class GmailEmailProvider : MimeEmailProvider
    {
        private readonly GmailOauthProvider _gmailOauthProvider;

        public GmailEmailProvider(GmailOauthProvider gmailOauthProvider)
        {
            _gmailOauthProvider = gmailOauthProvider;
        }
        
        protected override async Task SendMimeMessage(MimeMessage message, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var messageId = await _gmailOauthProvider.UploadDraft(message.ToString(), progressCallback, cancelToken);
            var userEmail = _gmailOauthProvider.User;
            // Open the draft in the user's browser
            // Note: As of this writing, the direct url is bugged in the new gmail UI, and there is no workaround
            // https://issuetracker.google.com/issues/113127519
            // At least it directs to the drafts folder
            Process.Start($"https://mail.google.com/mail/?authuser={userEmail}#drafts/{messageId}");
        }
    }
}
