namespace NAPS2.Scan;

public record BitDepthCaps(
    bool SupportsColor,
    bool SupportsGrayscale,
    bool SupportsBlackAndWhite
)
{
    private BitDepthCaps() : this(false, false, false)
    {
    }
}