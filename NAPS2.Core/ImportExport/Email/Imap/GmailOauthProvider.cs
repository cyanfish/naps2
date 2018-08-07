using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NAPS2.Config;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Imap
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

        public override OauthToken Token
        {
            get => userConfigManager.Config.EmailSetup?.GmailToken;
            protected set
            {
                userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
                userConfigManager.Config.EmailSetup.GmailToken = value;
                userConfigManager.Save();
            }
        }

        public override string User
        {
            get => userConfigManager.Config.EmailSetup?.GmailUser;
            protected set
            {
                userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
                userConfigManager.Config.EmailSetup.GmailUser = value;
                userConfigManager.Save();
            }
        }

        protected override OauthClientCreds Creds
        {
            get
            {
                if (creds == null)
                {
                    var credObj = JObject.Parse(Encoding.UTF8.GetString(ClientCreds.google_credentials));
                    var installed = credObj.Value<JObject>("installed");
                    creds = new OauthClientCreds(installed?.Value<string>("client_id"), installed?.Value<string>("client_secret"));
                }
                return creds;
            }
        }

        protected override string Scope => "https://www.googleapis.com/auth/gmail.compose";

        protected override string CodeEndpoint => "https://accounts.google.com/o/oauth2/v2/auth";

        protected override string TokenEndpoint => "https://www.googleapis.com/oauth2/v4/token";

        #endregion

        #region Api Methods

        protected override string GetUser()
        {
            var resp = GetAuthorized("https://www.googleapis.com/gmail/v1/users/me/profile");
            return resp.Value<string>("emailAddress");
        }

        public string UploadDraft(string messageRaw)
        {
            var resp = PostAuthorized($"https://www.googleapis.com/upload/gmail/v1/users/{User}/drafts?uploadType=multipart", messageRaw, "message/rfc822");
            return resp.Value<string>("id");
        }

        #endregion
    }
}
