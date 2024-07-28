namespace NAPS2.Scan;

public record ScanCaps(
    MetadataCaps? MetadataCaps,
    PaperSourceCaps? PaperSourceCaps,
    PerSourceCaps? FlatbedCaps,
    PerSourceCaps? FeederCaps,
    PerSourceCaps? DuplexCaps
)
{
    private ScanCaps() : this(null, null, null, null, null)
    {
    }
}