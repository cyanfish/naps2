namespace NAPS2.Scan;

/// <summary>
/// Scanning options specific to the Apple (ImageCaptureCore) driver.
/// </summary>
public class AppleOptions
{
    /// <summary>
    /// The transfer mode used for ImageCaptureCore, either File or Memory. By default Memory is used.
    /// </summary>
    public AppleTransferMode TransferMode { get; set; }
}

/// <summary>
/// The transfer mode to use for the Apple driver.
/// </summary>
public enum AppleTransferMode
{
    /// <summary>
    /// Transfers the image using the recommended mode. Usually this is Memory, but some scanners have known bugs with
    /// that mode, and where we can detect that we use File instead.
    /// </summary>
    Default,

    /// <summary>
    /// Transfers the image via a file written to a temp folder.
    /// </summary>
    File,

    /// <summary>
    /// Transfers the image in bands of memory.
    /// </summary>
    Memory
}
