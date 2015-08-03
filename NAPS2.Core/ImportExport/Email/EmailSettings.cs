using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public class EmailSettings
    {
        public EmailSettings()
        {
            AttachmentName = "Scan.pdf";
        }

        public string AttachmentName { get; set; }
    }
}
