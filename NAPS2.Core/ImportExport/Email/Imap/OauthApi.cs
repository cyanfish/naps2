using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Imap
{
    public abstract class OauthApi
    {
        public abstract OauthToken Token { get; }

        protected JObject Get(string url)
        {
            using (var client = ApiClient())
            {
                string response = client.DownloadString(url);
                return JObject.Parse(response);
            }
        }

        protected JObject Post(string url, NameValueCollection values)
        {
            using (var client = ApiClient())
            {
                string response = Encoding.UTF8.GetString(client.UploadValues(url, "POST", values));
                return JObject.Parse(response);
            }
        }

        protected JObject Post(string url, string body, string contentType)
        {
            using (var client = ApiClient())
            {
                client.Headers.Add("Content-Type", contentType);
                string response = client.UploadString(url, "POST", body);
                return JObject.Parse(response);
            }
        }

        private WebClient ApiClient()
        {
            var client = new WebClient();
            var token = Token;
            if (token != null)
            {
                // TODO: Refresh mechanism
                client.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            }
            return client;
        }
    }
}
