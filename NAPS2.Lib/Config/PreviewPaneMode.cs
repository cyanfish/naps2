using NAPS2.Scan;

namespace NAPS2.Config;

public enum PreviewPaneMode
{
    [LocalizedDescription(typeof(SettingsResources), "PreviewPaneMode_LastUsed")]
    LastUsed,
    [LocalizedDescription(typeof(SettingsResources), "PreviewPaneMode_PreviewPane")]
    PreviewPane,
    [LocalizedDescription(typeof(SettingsResources), "PreviewPaneMode_NewWindow")]
    NewWindow
}
