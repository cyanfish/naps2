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
    public class GmailApi : IOauthProvider
    {
        private const string OAUTH_URL_FORMAT =
            "https://accounts.google.com/o/oauth2/v2/auth?scope={0}&response_type=code&state={1}&redirect_uri={2}&client_id={3}";

        private const string OAUTH_SCOPE = "https://mail.google.com/"; //"https://www.googleapis.com/auth/gmail.compose";

        private const string OAUTH_EXCHANGE_URL = "https://www.googleapis.com/oauth2/v4/token";

        private const string PROFILE_URL = "https://www.googleapis.com/gmail/v1/users/me/profile";

        private readonly IUserConfigManager userConfigManager;

        public GmailApi(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public bool HasClientCreds => ClientId != null;

        private JObject CredData
        {
            get
            {
                var credString = Encoding.UTF8.GetString(ClientCreds.google_credentials);
                var credObj = JObject.Parse(credString);
                return credObj.Value<JObject>("installed");
            }
        }

        private string ClientId => CredData?.Value<string>("client_id");

        private string ClientSecret => CredData?.Value<string>("client_secret");

        public string OauthUrl(string state, string redirectUri)
        {
            // TODO: Check tls settings as in FDownloadProgress
            return string.Format(OAUTH_URL_FORMAT, OAUTH_SCOPE, state, redirectUri, ClientId);
        }

        public OauthToken AcquireToken(string code, string redirectUri)
        {
            using (var client = new WebClient())
            {
                var data = new NameValueCollection
                {
                    {"code", code},
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"redirect_uri", redirectUri},
                    {"grant_type", "authorization_code"}
                };
                string response = Encoding.UTF8.GetString(client.UploadValues(OAUTH_EXCHANGE_URL, "POST", data));
                var obj = JObject.Parse(response);
                return new OauthToken
                {
                    AccessToken = obj.Value<string>("access_token"),
                    RefreshToken = obj.Value<string>("refresh_token"),
                    Expiry = DateTime.Now.AddSeconds(obj.Value<int>("expires_in"))
                };
            }
        }

        public void RefreshToken(OauthToken token)
        {
            throw new NotImplementedException();
        }

        public string GetEmail(OauthToken token)
        {
            using (var client = new WebClient())
            {
                // TODO: Refresh mechanism
                client.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
                string response = client.DownloadString(PROFILE_URL);
                var obj = JObject.Parse(response);
                return obj.Value<string>("emailAddress");
            }
        }
    }
}
