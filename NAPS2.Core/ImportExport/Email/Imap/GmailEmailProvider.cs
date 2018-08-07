using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MailKit;
using MimeKit;
using NAPS2.Config;

namespace NAPS2.ImportExport.Email.Imap
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

        protected string Host => "imap.gmail.com";

        protected string User => userConfigManager.Config.EmailSetup?.GmailUser;

        protected string AccessToken => userConfigManager.Config.EmailSetup?.GmailToken?.AccessToken;

        protected void OpenDraft(IMailFolder drafts, UniqueId messageId)
        {
            drafts.Open(FolderAccess.ReadOnly);
            var gmailId = drafts.Fetch(new List<UniqueId> { messageId }, MessageSummaryItems.GMailMessageId).FirstOrDefault()?.GMailMessageId;
            var url = gmailId == null
                ? $"https://mail.google.com/mail/?authuser={User}#drafts" // This shouldn't happen, but handle it anyway
                : $"https://mail.google.com/mail/?authuser={User}#drafts?compose={gmailId:x}";
            Process.Start(url);
        }

        protected override void SendMimeMessage(MimeMessage message)
        {
            gmailOauthProvider.UploadDraft(message.ToString());//Convert.ToBase64String(Encoding.UTF8.GetBytes(message.ToString()))
        }
    }
}
