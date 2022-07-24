namespace NAPS2.Scan;

public class TwainOptions
{
    /// <summary>
    /// The DSM version of TWAIN to use. Drivers/Windows usually come bundled with an Old version, while NAPS2 itself
    /// provides a New version. This is the most common thing to try changing if you have compatibility issues. You
    /// can also use NewX64 to access 64-bit TWAIN drivers but usually they don't exist, everything is generally 32-bit. 
    /// </summary>
    public TwainDsm Dsm { get; set; }

    /// <summary>
    /// The adapter used for TWAIN, either the modern NTwain or the Legacy implementation used in very old versions of
    /// NAPS2. NTwain is better for the vast majority of scanners, but a few (e.g. Kyocera brand) may only work with
    /// Legacy for unknown reasons.  
    /// </summary>
    public TwainAdapter Adapter { get; set; }

    /// <summary>
    /// The transfer mode used for TWAIN, either Native or Memory. By default Native is used, but Memory might have
    /// work better with some scanners.
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

public enum TwainAdapter
{
    NTwain,
    Legacy
}

public enum TwainDsm
{
    New,
    // TODO: Consider dropping support for x64 twain, it's not tested and I don't think anyone can use it anyway
    NewX64,
    Old
}

public enum TwainTransferMode
{
    Native,
    Memory
}