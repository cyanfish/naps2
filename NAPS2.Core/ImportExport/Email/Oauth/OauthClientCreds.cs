using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class OauthClientCreds
    {
        public OauthClientCreds(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string ClientId { get; }

        public string ClientSecret { get; }
    }
}
