using NAPS2.Scan;

namespace NAPS2.Config;

public enum ScanButtonDefaultAction
{
    [LocalizedDescription(typeof(SettingsResources), "ScanButtonDefaultAction_ScanWithDefaultProfile")]
    ScanWithDefaultProfile,
    [LocalizedDescription(typeof(SettingsResources), "ScanButtonDefaultAction_AlwaysPrompt")]
    AlwaysPrompt
}