using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Util;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class GmailOauthProvider : OauthProvider
    {
        private readonly IUserConfigManager userConfigManager;

        private OauthClientCreds creds;

        public GmailOauthProvider(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        #region Authorization

        public override OauthToken Token => userConfigManager.Config.EmailSetup?.GmailToken;

        public override string User => userConfigManager.Config.EmailSetup?.GmailUser;

        protected override OauthClientCreds ClientCreds
        {
            get
            {
                if (creds == null)
                {
                    var credObj = JObject.Parse(Encoding.UTF8.GetString(NAPS2.ClientCreds.google_credentials));
                    var installed = credObj.Value<JObject>("installed");
                    creds = new OauthClientCreds(installed?.Value<string>("client_id"), installed?.Value<string>("client_secret"));
                }
                return creds;
            }
        }

        protected override string Scope => "https://www.googleapis.com/auth/gmail.compose";

        protected override string CodeEndpoint => "https://accounts.google.com/o/oauth2/v2/auth";

        protected override string TokenEndpoint => "https://www.googleapis.com/oauth2/v4/token";

        protected override void SaveToken(OauthToken token, bool refresh)
        {
            userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
            userConfigManager.Config.EmailSetup.GmailToken = token;
            if (!refresh)
            {
                userConfigManager.Config.EmailSetup.GmailUser = GetEmailAddress();
                userConfigManager.Config.EmailSetup.ProviderType = EmailProviderType.Gmail;
            }
            userConfigManager.Save();
        }

        #endregion

        #region Api Methods

        public string GetEmailAddress()
        {
            var resp = GetAuthorized("https://www.googleapis.com/gmail/v1/users/me/profile");
            return resp.Value<string>("emailAddress");
        }

        public async Task<DraftInfo> UploadDraft(string messageRaw, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var resp = await PostAuthorized($"https://www.googleapis.com/upload/gmail/v1/users/{User}/drafts?uploadType=multipart",
                messageRaw,
                "message/rfc822",
                progressCallback,
                cancelToken);
            return new DraftInfo
            {
                MessageId = resp.Value<JObject>("message").Value<string>("id"),
                DraftId = resp.Value<string>("id")
            };
        }

        public async Task SendDraft(string draftId)
        {
            await PostAuthorized($"https://www.googleapis.com/gmail/v1/users/{User}/drafts/send",
                new JObject
                {
                    { "id", draftId }
                }.ToString(), 
                "application/json",
                (current, max) => { },
                CancellationToken.None);
        }

        public class DraftInfo
        {
            public string DraftId { get; set; }
            
            public string MessageId { get; set; }
        }

        #endregion
    }
}
