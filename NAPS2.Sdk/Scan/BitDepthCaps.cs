namespace NAPS2.Scan;

/// <summary>
/// Represents valid values for ScanOptions.BitDepth as part of PerSourceCaps.
/// </summary>
public class BitDepthCaps
{
    /// <summary>
    /// Whether the scanner supports BitDepth.Color.
    /// </summary>
    public bool SupportsColor { get; init; }

    /// <summary>
    /// Whether the scanner supports BitDepth.Grayscale.
    /// </summary>
    public bool SupportsGrayscale { get; init; }

    /// <summary>
    /// Whether the scanner supports BitDepth.BlackAndWhite.
    /// </summary>
    public bool SupportsBlackAndWhite { get; init; }
}