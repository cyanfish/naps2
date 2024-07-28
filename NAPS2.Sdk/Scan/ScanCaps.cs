namespace NAPS2.Scan;

public class ScanCaps
{
    internal ScanCaps()
    {
    }

    public MetadataCaps? MetadataCaps { get; init; }

    public PaperSourceCaps? PaperSourceCaps { get; init; }

    public PerSourceCaps? FlatbedCaps { get; init; }

    public PerSourceCaps? FeederCaps { get; init; }

    public PerSourceCaps? DuplexCaps { get; init; }
}