namespace NAPS2.Images.Bitwise;

public struct PixelInfo
{
    /// <summary>
    /// Represents R-G-B subpixel ordering, with 8 bits per component (24 total).
    /// </summary>
    public static unsafe PixelInfo Rgb(byte* data, int stride, int w, int h) => new()
    {
        data = data,
        stride = stride,
        w = w,
        h = h,
        bitsPerPixel = 24,
        bytesPerPixel = 3,
        rOff = 0,
        gOff = 1,
        bOff = 2
    };

    /// <summary>
    /// Represents B-G-R subpixel ordering, with 8 bits per component (24 total).
    /// </summary>
    public static unsafe PixelInfo Bgr(byte* data, int stride, int w, int h) => new()
    {
        data = data,
        stride = stride,
        w = w,
        h = h,
        bitsPerPixel = 24,
        bytesPerPixel = 3,
        rOff = 2,
        gOff = 1,
        bOff = 0
    };

    /// <summary>
    /// Represents R-G-B-A subpixel ordering, with 8 bits per component (32 total).
    /// </summary>
    public static unsafe PixelInfo Rgba(byte* data, int stride, int w, int h) => new()
    {
        data = data,
        stride = stride,
        w = w,
        h = h,
        bitsPerPixel = 32,
        bytesPerPixel = 4,
        rOff = 0,
        gOff = 1,
        bOff = 2,
        aOff = 3
    };

    /// <summary>
    /// Represents B-G-R-A subpixel ordering, with 8 bits per component (32 total).
    /// </summary>
    public static unsafe PixelInfo Bgra(byte* data, int stride, int w, int h) => new()
    {
        data = data,
        stride = stride,
        w = w,
        h = h,
        bitsPerPixel = 32,
        bytesPerPixel = 4,
        rOff = 2,
        gOff = 1,
        bOff = 0,
        aOff = 3
    };

    /// <summary>
    /// Represents 8 bit grayscale.
    /// </summary>
    public static unsafe PixelInfo Gray(byte* data, int stride, int w, int h) => new()
    {
        data = data,
        stride = stride,
        w = w,
        h = h,
        bitsPerPixel = 8,
        bytesPerPixel = 1
    };

    public unsafe byte* data;
    public int stride;
    public int w;
    public int h;
    public int bitsPerPixel;
    public int bytesPerPixel;
    public int rOff;
    public int gOff;
    public int bOff;
    public int aOff;

    public (int, int, int, int, int) BitLayout => (bitsPerPixel, rOff, gOff, bOff, aOff);
}