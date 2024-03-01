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

    /// <summary>
    /// Whether to keep SANE initialized in memory after the operation is complete. This improves stability when
    /// doing multiple operations in a single process. Defaults to true.
    /// </summary>
    public bool KeepInitialized { get; set; } = true;
}