using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using NAPS2.Config;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class GmailEmailProvider : MimeEmailProvider
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly GmailOauthProvider gmailOauthProvider;

        public GmailEmailProvider(IUserConfigManager userConfigManager, GmailOauthProvider gmailOauthProvider)
        {
            this.userConfigManager = userConfigManager;
            this.gmailOauthProvider = gmailOauthProvider;
        }
        
        protected override async Task SendMimeMessage(MimeMessage message, ProgressHandler progressCallback, CancellationToken cancelToken,
            bool autoSend)
        {
            var draft = await gmailOauthProvider.UploadDraft(message.ToString(), progressCallback, cancelToken);
            if (autoSend)
            {
                await gmailOauthProvider.SendDraft(draft.DraftId);
            }
            else
            {
                var userEmail = userConfigManager.Config.EmailSetup?.GmailUser;
                // Open the draft in the user's browser
                // Note: As of this writing, the direct url is bugged in the new gmail UI, and there is no workaround
                // https://issuetracker.google.com/issues/113127519
                // At least it directs to the drafts folder
                Process.Start($"https://mail.google.com/mail/?authuser={userEmail}#drafts/{draft.MessageId}");
            }
        }
    }
}
