using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Images
{
    public enum TiffCompression
    {
        [LocalizedDescription(typeof(SettingsResources), "TiffComp_Auto")]
        Auto,
        [LocalizedDescription(typeof(SettingsResources), "TiffComp_Lzw")]
        Lzw,
        [LocalizedDescription(typeof(SettingsResources), "TiffComp_Ccitt4")]
        Ccitt4,
        [LocalizedDescription(typeof(SettingsResources), "TiffComp_None")]
        None
    }
}