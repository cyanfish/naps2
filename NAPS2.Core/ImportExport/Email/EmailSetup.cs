using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.ImportExport.Email.Oauth;

namespace NAPS2.ImportExport.Email
{
    public class EmailSetup
    {
        // TODO: Should more of this be encrypted? Probably just pw/token is fine.
        // Also, how would this work securely from an appsettings pov?
        // Remove the "Secure" suffix here. Then encrypt when mapping from appsettings.
        // Consider some kind of prefix for the encrypted text so anyone looking at the xml knows it is encrypted. "secure-xxx" "encrypted:xxx"
        // Would also allow an error when mapping from appsettings if they copy the wrong thing.
        // Consider how to behave if ProviderType is set (e.g. to gmail) but the user/token isn't.
        public EmailProviderType ProviderType { get; set; }

        public string SystemProviderName { get; set; }

        public string GmailUser { get; set; }

        public OauthToken GmailToken { get; set; }

        public string OutlookWebUser { get; set; }

        public OauthToken OutlookWebToken { get; set; }

        public string SmtpHost { get; set; }

        public string SmtpFrom { get; set; }

        public int? SmtpPort { get; set; }

        public bool SmtpTls { get; set; }

        public string SmtpUser { get; set; }

        public string SmtpPassword { get; set; }
    }
}
