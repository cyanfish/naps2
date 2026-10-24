using NAPS2.Scan;

namespace NAPS2.Config;

public enum DockStyle
{
    [LocalizedDescription(typeof(SettingsResources), "DockStyle_Top")]
    Top,
    [LocalizedDescription(typeof(SettingsResources), "DockStyle_Left")]
    Left,
    [LocalizedDescription(typeof(SettingsResources), "DockStyle_Bottom")]
    Bottom,
    [LocalizedDescription(typeof(SettingsResources), "DockStyle_Right")]
    Right
}