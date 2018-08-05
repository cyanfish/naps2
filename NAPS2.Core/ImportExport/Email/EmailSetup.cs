using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.ImportExport.Email
{
    public class EmailSetup
    {
        public EmailProviderType ProviderType { get; set; }

        public string GmailUser { get; set; }

        public string OutlookWebUser { get; set; }

        public string SmtpUser { get; set; }
    }
}
