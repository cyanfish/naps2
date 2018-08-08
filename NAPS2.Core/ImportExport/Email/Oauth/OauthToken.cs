using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class OauthToken
    {
        public SecureString AccessToken { get; set; }

        public SecureString RefreshToken { get; set; }

        public DateTime Expiry { get; set; }
    }
}