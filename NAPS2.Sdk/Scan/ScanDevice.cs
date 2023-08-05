namespace NAPS2.Scan;

/// <summary>
/// The representation of a scanning device identified by a driver.
/// </summary>
public record ScanDevice(string ID, string Name)
{
    private ScanDevice() : this("", "")
    {
    }
}