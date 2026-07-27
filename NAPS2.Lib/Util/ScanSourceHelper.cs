using NAPS2.EtoForms.Widgets;
using NAPS2.Scan;

namespace NAPS2.Util;

public static class ScanSourceHelper
{
    public static List<ScanSource> GetCompatibleScanSources(ScanProfile profile, Driver deviceDriver)
    {
        if(profile.Caps?.PaperSources?.Values is not { } scanSources)
            scanSources = [.. EnumDropDownWidget<ScanSource>.DefaultItems];
        if (deviceDriver is not (Driver.Wia or Driver.Twain or Driver.Escl))
            scanSources.Remove(ScanSource.Auto);
        return scanSources;
    }
}