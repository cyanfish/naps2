namespace NAPS2.Scan;

/// <summary>
/// Scanning options specific to the TWAIN driver.
/// </summary>
public class TwainOptions
{
    /// <summary>
    /// The DSM version of TWAIN to use. Drivers/Windows usually come bundled with an Old version, while NAPS2 itself
    /// provides a New version. This is the most common thing to try changing if you have compatibility issues. You
    /// can also use NewX64 to access 64-bit TWAIN drivers but usually they don't exist, everything is generally 32-bit. 
    /// </summary>
    public TwainDsm Dsm { get; set; }

    /// <summary>
    /// The transfer mode used for TWAIN, either Native or Memory. By default Memory is used.
    /// </summary>
    public TwainTransferMode TransferMode { get; set; }

    /// <summary>
    /// Whether to show the TWAIN progress UI. This only matters when ScanOptions.UseNativeUI is false (otherwise the
    /// full UI is shown regardless).
    /// </summary>
    public bool ShowProgress { get; set; }

    /// <summary>
    /// Whether to include include devices that start with "WIA-" in GetDeviceList.
    /// Windows makes WIA devices available to TWAIN applications through a translation layer.
    /// By default they are excluded, since NAPS2 supports using WIA devices directly.
    /// </summary>
    public bool IncludeWiaDevices { get; set; }
}

/// <summary>
/// The data source manager (DSM) to use for TWAIN.
/// </summary>
public enum TwainDsm
{
    /// <summary>
    /// The modern 32-bit twaindsm.dll. Recommended.
    /// </summary>
    New,

    /// <summary>
    /// The modern 64-bit twaindsm.dll. Choose this if you want to use a 64-bit TWAIN data source.
    /// </summary>
    NewX64,

    /// <summary>
    /// The old 32-bit twain32.dll. Some data sources have compatibility issues with the newer DSM.
    /// </summary>
    Old
}

/// <summary>
/// The transfer mode to use for TWAIN.
/// </summary>
public enum TwainTransferMode
{
    /// <summary>
    /// Transfers the image in strips. Recommended.
    /// </summary>
    Memory,

    /// <summary>
    /// Transfers the entire image at once. This may fail with very high-resolution images if they exceed the memory
    /// limits of the 32-bit worker.
    /// </summary>
    Native
}