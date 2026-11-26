using NAPS2.Scan;

namespace NAPS2.Config;

public enum AppTheme
{
    [LocalizedDescription(typeof(SettingsResources), "Theme_Default")]
    Default,
    [LocalizedDescription(typeof(SettingsResources), "Theme_Light")]
    Light,
    [LocalizedDescription(typeof(SettingsResources), "Theme_Dark")]
    Dark
}