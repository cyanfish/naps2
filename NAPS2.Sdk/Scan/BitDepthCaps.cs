namespace NAPS2.Scan;

public class BitDepthCaps
{
    internal BitDepthCaps()
    {
    }

    public bool SupportsColor { get; init; }

    public bool SupportsGrayscale { get; init; }

    public bool SupportsBlackAndWhite { get; init; }
}