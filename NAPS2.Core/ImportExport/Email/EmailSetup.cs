using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.ImportExport.Email
{
    public class EmailSetup
    {
        public EmailProviderType ProviderType { get; set; }

        public string SystemAppName { get; set; }

        public string GmailUser { get; set; }

        public string GmailTokenSecure { get; set; }

        public string OutlookWebUser { get; set; }

        public string OutlookWebTokenSecure { get; set; }

        public string SmtpHost { get; set; }

        public string SmtpFrom { get; set; }

        public int? SmtpPort { get; set; }

        public bool SmtpTls { get; set; }

        public string SmtpUser { get; set; }

        public string SmtpPasswordSecure { get; set; }
    }
}
