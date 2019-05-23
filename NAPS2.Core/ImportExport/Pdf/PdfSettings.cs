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

        public bool SinglePagePdf { get; set; }

        public PdfMetadata Metadata
        {
            get => metadata;
            set => metadata = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PdfEncryption Encryption
        {
            get => encryption;
            set => encryption = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PdfCompat Compat { get; set; }
    }
}
