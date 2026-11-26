namespace NAPS2.Images;

/// <summary>
/// Encodes JPEG images with jpegli (via the naps2_jpeg native library), which produces significantly smaller files
/// than the platform encoders at the same quality.
/// </summary>
internal class JpegliCodec : AbstractBundledCodec
{
    public static JpegliCodec? TryCreate() => TryCreate(lib => new JpegliCodec(lib));

    private JpegliCodec(JpegNativeLibrary lib)
        // Grayscale JPEGs can't be embedded directly in PDFs
        : base(lib, ImageFileFormat.Jpeg, allowGray: false)
    {
    }

    protected override int? GetDirectPixelFormat(BitwiseImageData data)
    {
        if (data.invertY || data.invertColorSpace || data.premultipliedAlpha || data.stride <= 0)
        {
            return null;
        }
        return (data.bytesPerPixel, data.rOff, data.gOff, data.bOff) switch
        {
            (3, 0, 1, 2) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGB24,
            (3, 2, 1, 0) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_BGR24,
            // The 4th byte (alpha or padding) is ignored
            (4, 0, 1, 2) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGBA32,
            (4, 2, 1, 0) => JpegNativeLibrary.NAPS2_JPEG_PIXEL_BGRA32,
            _ => null
        };
    }

    protected override int Compress(IMemoryImage image, ImageSaveOptions options, IntPtr pixels, int stride,
        int pixelFormat, out IntPtr outData, out UIntPtr outSize, byte[] error) =>
        Lib.Compress(pixels, image.Width, image.Height, stride, pixelFormat, GetQuality(options),
            image.HorizontalResolution, image.VerticalResolution, out outData, out outSize, error, error.Length);
}
