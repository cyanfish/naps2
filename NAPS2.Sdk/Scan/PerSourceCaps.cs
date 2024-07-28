namespace NAPS2.Scan;

public class PerSourceCaps
{
    internal PerSourceCaps()
    {
    }

    public DpiCaps? DpiCaps { get; init; }

    public BitDepthCaps? BitDepthCaps { get; init; }

    public PageSizeCaps? PageSizeCaps { get; init; }
}