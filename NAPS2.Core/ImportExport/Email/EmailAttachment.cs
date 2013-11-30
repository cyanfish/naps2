using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public class EmailAttachment
    {
        /// <summary>
        /// The path of the source file to be attached.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the attachment (usually the source file name).
        /// </summary>
        public string AttachmentName { get; set; }
    }
}