using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Scan;

// ScanDevice used to only have ID and Name, but now it has Driver too. We need ScanProfileDevice for backwards compat
// when serializing.
public record ScanProfileDevice(string ID, string Name)
{
    [return: NotNullIfNotNull("device")]
    public static ScanProfileDevice? FromScanDevice(ScanDevice? device)
    {
        if (device == null)
        {
            return null;
        }
        return new ScanProfileDevice(device.ID, device.Name);
    }

    public ScanDevice ToScanDevice(Driver driver)
    {
        return new ScanDevice(driver, ID, Name);
    }

    private ScanProfileDevice() : this("", "")
    {
    }
}