using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.Config;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettingsContainer
    {
        private readonly UserConfigManager userConfigManager;

        private PdfSettings localPdfSettings;

        public PdfSettingsContainer(UserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public PdfSettings PdfSettings
        {
            get { return localPdfSettings ?? userConfigManager.Config.PdfSettings ?? new PdfSettings(); }
            set { localPdfSettings = value; }
        }
    }
}
