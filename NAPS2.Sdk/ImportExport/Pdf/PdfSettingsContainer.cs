using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettingsContainer : PdfSettingsProvider
    {
        public override PdfSettings PdfSettings => LocalPdfSettings ?? UserConfig.Current.PdfSettings ?? new PdfSettings();

        public PdfSettings LocalPdfSettings { get; set; }
    }
}
