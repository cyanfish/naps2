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

        public string AttachmentName { get; set; }
    }
}
