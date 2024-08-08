namespace NAPS2.Scan;

/// <summary>
/// Represents valid values for ScanOptions.PaperSource as part of ScanCaps.
/// </summary>
public class PaperSourceCaps
{
    /// <summary>
    /// Whether the scanner supports PaperSource.Flatbed.
    /// </summary>
    public bool SupportsFlatbed { get; init; }

    /// <summary>
    /// Whether the scanner supports PaperSource.Feeder.
    /// </summary>
    public bool SupportsFeeder { get; init; }

    /// <summary>
    /// Whether the scanner supports PaperSource.Duplex.
    /// </summary>
    public bool SupportsDuplex { get; init; }

    /// <summary>
    /// Whether the scanner has the ability to detect if paper is in the feeder for use with PaperSource.Auto.
    /// </summary>
    public bool CanCheckIfFeederHasPaper { get; init; }
}