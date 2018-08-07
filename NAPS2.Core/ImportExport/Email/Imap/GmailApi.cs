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
    public partial class GmailApi : OauthApi, IOauthProvider
    {
        private const string OAUTH_SCOPE = "https://www.googleapis.com/auth/gmail.compose";

        private readonly IUserConfigManager userConfigManager;

        private OauthClientCreds creds;

        public GmailApi(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public override OauthToken Token => userConfigManager.Config.EmailSetup?.GmailToken;

        public string UserId => userConfigManager.Config.EmailSetup?.GmailUser;

        public bool HasClientCreds => Creds.ClientId != null;

        private OauthClientCreds Creds
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

        public string OauthUrl(string state, string redirectUri)
        {
            // TODO: Check tls settings as in FDownloadProgress
            return "https://accounts.google.com/o/oauth2/v2/auth?"
                   + $"scope={OAUTH_SCOPE}&response_type=code&state={state}&redirect_uri={redirectUri}&client_id={Creds.ClientId}";
        }

        public OauthToken AcquireToken(string code, string redirectUri)
        {
            var resp = Post("https://www.googleapis.com/oauth2/v4/token", new NameValueCollection
            {
                {"code", code},
                {"client_id", Creds.ClientId},
                {"client_secret", Creds.ClientSecret},
                {"redirect_uri", redirectUri},
                {"grant_type", "authorization_code"}
            });
            return new OauthToken
            {
                AccessToken = resp.Value<string>("access_token"),
                RefreshToken = resp.Value<string>("refresh_token"),
                Expiry = DateTime.Now.AddSeconds(resp.Value<int>("expires_in"))
            };
        }

        public void RefreshToken()
        {
            throw new NotImplementedException();
        }

        public string GetEmail()
        {
            var resp = Get("https://www.googleapis.com/gmail/v1/users/me/profile");
            return resp.Value<string>("emailAddress");
        }

        public string UploadDraft(string messageRaw)
        {
            var resp = Post($"https://www.googleapis.com/upload/gmail/v1/users/{UserId}/drafts?uploadType=multipart", messageRaw, "message/rfc822");
            return resp.Value<string>("id");
        }
    }
}
