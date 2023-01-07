namespace NAPS2.Images.Bitwise;

/// <summary>
/// The normal 0-255 RGB values we use are in the "intensity" space. We can use a gamma conversion
/// to convert these to the "luminance" space, which is better for certain kinds of image processing.
/// Then convert back to the "intensity" space once we're done. This class provides pre-calculated tables
/// for these conversions.
/// https://www.benq.com/en-ca/knowledge-center/knowledge/gamma-monitor.html
/// </summary>
internal static class GammaTables
{
    // Doing math in the 0-255 integral space means there's a significant loss of precision.
    // Instead we can work in the 0-MAX_LUM space to give more granularity without needing to use slower floating point.
    public const int MULTIPLIER = 16;
    public const int MAX_LUM = 255 * MULTIPLIER;

    public static short[] IntensityToLum { get; }
    public static byte[] LumToIntensity { get; }

    static GammaTables()
    {
        const double gamma = 2.2;
        IntensityToLum = new short[256];
        LumToIntensity = new byte[MAX_LUM + 1];
        for (int x = 0; x < 256; x++)
        {
            var i = Math.Pow(x / 255.0, 1 / gamma);
            IntensityToLum[x] = (short) Math.Round(i * MAX_LUM);
        }
        for (int x = 0; x <= MAX_LUM; x++)
        {
            var l = Math.Pow(x / (double) MAX_LUM, gamma);
            LumToIntensity[x] = (byte) Math.Round(l * 255);
        }
    }
}