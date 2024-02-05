namespace NAPS2.Scan;

/// <summary>
/// Scanning options specific to the SANE driver.
/// </summary>
public class SaneOptions
{
    /// <summary>
    /// Limit the devices queried by GetDevices/GetDeviceList to the given SANE backend.
    /// </summary>
    public string? Backend { get; set; }
}