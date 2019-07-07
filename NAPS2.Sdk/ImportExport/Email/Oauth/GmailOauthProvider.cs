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
    // TODO: The config references should be pulled elsewhere so this can be included in the SDK
    public class GmailOauthProvider : OauthProvider
    {
        private readonly ConfigScopes configScopes;
        private readonly ConfigProvider<CommonConfig> configProvider;

        private OauthClientCreds creds;

        public GmailOauthProvider(ConfigScopes configScopes, ConfigProvider<CommonConfig> configProvider)
        {
            this.configScopes = configScopes;
            this.configProvider = configProvider;
        }

        #region Authorization

        public override OauthToken Token => configProvider.Get(c => c.EmailSetup.GmailToken);

        public override string User => configProvider.Get(c => c.EmailSetup.GmailUser);

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
            var emailSetup = configProvider.Get(c => c.EmailSetup);
            emailSetup.GmailToken = token;
            if (!refresh)
            {
                emailSetup.GmailUser = GetEmailAddress();
                emailSetup.ProviderType = EmailProviderType.Gmail;
            }
            configScopes.User.Set(c => c.EmailSetup = emailSetup);
        }

        #endregion

        #region Api Methods

        public string GetEmailAddress()
        {
            var resp = GetAuthorized("https://www.googleapis.com/gmail/v1/users/me/profile");
            return resp.Value<string>("emailAddress");
        }

        public async Task<string> UploadDraft(string messageRaw, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var resp = await PostAuthorized($"https://www.googleapis.com/upload/gmail/v1/users/{User}/drafts?uploadType=multipart",
                messageRaw,
                "message/rfc822",
                progressCallback,
                cancelToken);
            return resp.Value<JObject>("message").Value<string>("id");
        }

        #endregion
    }
}
