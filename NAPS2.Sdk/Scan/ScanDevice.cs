namespace NAPS2.Scan;

/// <summary>
/// The representation of a scanning device and its corresponding driver.
/// </summary>
public record ScanDevice(Driver Driver, string ID, string Name)
{
    private ScanDevice() : this(Driver.Default, "", "")
    {
    }
}