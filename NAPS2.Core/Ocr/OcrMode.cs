using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.Ocr
{
    public enum OcrMode
    {
        Default,
        [LocalizedDescription(typeof(SettingsResources), "OcrMode_Fast")]
        Fast,
        [LocalizedDescription(typeof(SettingsResources), "OcrMode_Best")]
        Best,
        [LocalizedDescription(typeof(SettingsResources), "OcrMode_Legacy")]
        Legacy
    }
}
