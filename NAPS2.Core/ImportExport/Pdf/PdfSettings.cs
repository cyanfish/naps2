using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettings
    {
        private PdfMetadata metadata;
        private PdfEncryption encryption;

        public PdfSettings()
        {
            metadata = new PdfMetadata();
            encryption = new PdfEncryption();
        }

        public string DefaultFileName { get; set; }

        public bool SkipSavePrompt { get; set; }

        public PdfMetadata Metadata
        {
            get { return metadata; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                metadata = value;
            }
        }

        public PdfEncryption Encryption
        {
            get { return encryption; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                encryption = value;
            }
        }
    }
}
