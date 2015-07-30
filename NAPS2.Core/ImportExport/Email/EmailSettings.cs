using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.ImportExport.Email
{
    public class EmailSettings
    {
        public EmailSettings()
        {
            AttachmentName = "Scan.pdf";
        }

        public string To { get; set; }

        public string Cc { get; set; }

        public string Bcc { get; set; }

        public string Subject { get; set; }

        public string AttachmentName { get; set; }

        public string BodyText { get; set; }
    }
}
