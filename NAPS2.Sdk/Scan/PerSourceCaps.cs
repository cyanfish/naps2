namespace NAPS2.Scan;

public record PerSourceCaps(
    DpiCaps? DpiCaps,
    BitDepthCaps? BitDepthCaps,
    PageSizeCaps? PageSizeCaps
)
{
    private PerSourceCaps() : this(null, null, null)
    {
    }
}