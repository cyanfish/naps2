using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MailKit;
using MimeKit;
using NAPS2.Config;

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
        
        protected override void SendMimeMessage(MimeMessage message)
        {
            var messageId = gmailOauthProvider.UploadDraft(message.ToString());
            var userEmail = userConfigManager.Config.EmailSetup?.GmailUser;
            // Open the draft in the user's browser
            Process.Start($"https://mail.google.com/mail/?authuser={userEmail}#drafts?compose={messageId}");
        }
    }
}
