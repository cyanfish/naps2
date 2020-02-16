using System;
using NAPS2.Config;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettings
    {
        private PdfMetadata _metadata;
        private PdfEncryption _encryption;

        public PdfSettings()
        {
            _metadata = new PdfMetadata();
            _encryption = new PdfEncryption();
        }

        public string? DefaultFileName { get; set; }

        public bool? SkipSavePrompt { get; set; }

        [Child]
        public PdfMetadata Metadata
        {
            get => _metadata;
            set => _metadata = value ?? throw new ArgumentNullException(nameof(value));
        }

        [Child]
        public PdfEncryption Encryption
        {
            get => _encryption;
            set => _encryption = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PdfCompat? Compat { get; set; }
    }
}
