using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MailKit;
using NAPS2.Config;

namespace NAPS2.ImportExport.Email.Imap
{
    public class GmailEmailProvider : ImapEmailProvider
    {
        private readonly IUserConfigManager userConfigManager;

        public GmailEmailProvider(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        protected override string Host => "imap.gmail.com";

        protected override string User => userConfigManager.Config.EmailSetup?.GmailUser;

        protected override string AccessToken => ""; // TODO

        protected override void OpenDraft(IMailFolder drafts, UniqueId messageId)
        {
            drafts.Open(FolderAccess.ReadOnly);
            var gmailId = drafts.Fetch(new List<UniqueId> { messageId }, MessageSummaryItems.GMailMessageId).FirstOrDefault()?.GMailMessageId;
            var url = gmailId == null
                ? $"https://mail.google.com/mail/?authuser={User}#drafts" // This shouldn't happen, but handle it anyway
                : $"https://mail.google.com/mail/?authuser={User}#drafts?compose={gmailId:x}";
            Process.Start(url);
        }
    }
}
