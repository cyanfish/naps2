using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Pdf
{
    public enum PdfCompat
    {
        [LocalizedDescription(typeof(SettingsResources), "PdfCompat_Default")]
        Default,
        [LocalizedDescription(typeof(SettingsResources), "PdfCompat_NoInterp")]
        NoInterp,
        [LocalizedDescription(typeof(SettingsResources), "PdfCompat_PdfA1B")]
        PdfA1B
    }
}
