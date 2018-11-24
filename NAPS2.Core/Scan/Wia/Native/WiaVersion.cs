using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Wia.Native
{
    public enum WiaVersion
    {
        [LocalizedDescription(typeof(SettingsResources), "WiaVersion_Default")]
        Default,
        [LocalizedDescription(typeof(SettingsResources), "WiaVersion_Wia10")]
        Wia10,
        [LocalizedDescription(typeof(SettingsResources), "WiaVersion_Wia20")]
        Wia20
    }
}
