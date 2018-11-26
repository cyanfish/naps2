using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettingsContainer
    {
        private PdfSettings localPdfSettings;

        public PdfSettings PdfSettings
        {
            get => localPdfSettings ?? UserConfig.Current.PdfSettings ?? new PdfSettings();
            set => localPdfSettings = value;
        }
    }
}
