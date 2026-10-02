using NAPS2.Scan;

namespace NAPS2.EtoForms.Desktop;

public interface IDesktopScanController
{
    /// <summary>
    /// Whether a scan started from the desktop UI is currently in progress.
    /// </summary>
    bool IsScanning { get; }

    event EventHandler? IsScanningChanged;

    Task ScanWithDevice(string deviceID);
    Task ScanDefault();
    Task ScanWithNewProfile();
    Task ScanWithProfile(ScanProfile profile);
}