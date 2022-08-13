namespace NAPS2.Images.Bitwise;

public class SubPixelType
{
    public static readonly SubPixelType Rgba = new()
    {
        BitsPerPixel = 32,
        BytesPerPixel = 4,
        RedOffset = 0,
        GreenOffset = 1,
        BlueOffset = 2,
        AlphaOffset = 3
    };

    public static readonly SubPixelType Rgb = new()
    {
        BitsPerPixel = 24,
        BytesPerPixel = 3,
        RedOffset = 0,
        GreenOffset = 1,
        BlueOffset = 2
    };

    public static readonly SubPixelType Bgra = new()
    {
        BitsPerPixel = 32,
        BytesPerPixel = 4,
        RedOffset = 2,
        GreenOffset = 1,
        BlueOffset = 0,
        AlphaOffset = 3
    };

    public static readonly SubPixelType Bgr = new()
    {
        BitsPerPixel = 24,
        BytesPerPixel = 3,
        RedOffset = 2,
        GreenOffset = 1,
        BlueOffset = 0
    };

    public static readonly SubPixelType Gray = new()
    {
        BitsPerPixel = 8,
        BytesPerPixel = 1
    };

    // TODO: We probably need to handle bit inversions (i.e. white = 0, black = 1)
    public static readonly SubPixelType Bit = new()
    {
        BitsPerPixel = 1
    };

    private SubPixelType()
    {
    }

    public int BitsPerPixel { get; private init; }
    public int BytesPerPixel { get; private init; }
    public int RedOffset { get; private init; }
    public int GreenOffset { get; private init; }
    public int BlueOffset { get; private init; }
    public int AlphaOffset { get; private init; }
}