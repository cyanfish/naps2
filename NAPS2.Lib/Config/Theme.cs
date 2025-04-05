using NAPS2.Scan;

namespace NAPS2.Config;

public enum Theme
{
    [LocalizedDescription(typeof(SettingsResources), "Theme_Default")]
    Default,
    [LocalizedDescription(typeof(SettingsResources), "Theme_Light")]
    Light,
    [LocalizedDescription(typeof(SettingsResources), "Theme_Dark")]
    Dark
}