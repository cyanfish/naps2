namespace NAPS2.Images.Bitwise;

internal static class GammaTables
{
    public static byte[] IntensityToLum { get; }
    public static byte[] LumToIntensity { get; }

    static GammaTables()
    {
        const double gamma = 2.2;
        IntensityToLum = new byte[256];
        LumToIntensity = new byte[256];
        for (int x = 0; x < 256; x++)
        {
            var i = Math.Pow(x / 255.0, 1 / gamma);
            IntensityToLum[x] = (byte) Math.Round(i * 255);
            var l = Math.Pow(x / 255.0, gamma);
            LumToIntensity[x] = (byte) Math.Round(l * 255);
        }
    }
}