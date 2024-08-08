namespace NAPS2.Scan;

/// <summary>
/// Represents scanner capabilities. This includes valid values for scanning options and extra metadata beyond just the
/// device name and id.
/// </summary>
public class ScanCaps
{
    /// <summary>
    /// Metadata for the device.
    /// </summary>
    public MetadataCaps? MetadataCaps { get; init; }

    /// <summary>
    /// Valid values for ScanOptions.PaperSource.
    /// </summary>
    public PaperSourceCaps? PaperSourceCaps { get; init; }

    /// <summary>
    /// Capabilities specific to the Flatbed paper source.
    /// </summary>
    public PerSourceCaps? FlatbedCaps { get; init; }

    /// <summary>
    /// Capabilities specific to the Feeder paper source.
    /// </summary>
    public PerSourceCaps? FeederCaps { get; init; }

    /// <summary>
    /// Capabilities specific to the Duplex paper source.
    /// </summary>
    public PerSourceCaps? DuplexCaps { get; init; }
}