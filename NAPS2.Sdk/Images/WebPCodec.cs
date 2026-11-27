using NAPS2.ImportExport;

namespace NAPS2.Images;

/// <summary>
/// Encodes and decodes WebP images with libwebp (via the naps2_jpeg native library).
/// </summary>
internal class WebPCodec : AbstractBundledCodec
{
    public static WebPCodec? TryCreate() => TryCreate(lib => new WebPCodec(lib));

    private readonly JpegNativeLibrary.naps2_webp_compress_delegate _compress;
    private readonly JpegNativeLibrary.naps2_webp_decompress_delegate _decompress;

    private WebPCodec(JpegNativeLibrary lib)
        : base(lib, ImageFileFormat.WebP,
            // WebP has no grayscale mode
            allowGray: false)
    {
        _compress = lib.WebpCompress;
        _decompress = lib.WebpDecompress;
    }

    protected override int? GetDirectPixelFormat(BitwiseImageData data)
    {
        if (data.invertY || data.invertColorSpace || data.premultipliedAlpha)
        {
            return null;
        }
        return (data.bytesPerPixel, data.rOff, data.gOff, data.bOff, data.hasAlpha) switch
        {
            (3, 0, 1, 2, false) when data.stride == data.w * 3 => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGB24,
            (4, 0, 1, 2, true) when data.stride == data.w * 4 => JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGBA32,
            _ => null
        };
    }

    protected override int Compress(IMemoryImage image, ImageSaveOptions options, IntPtr pixels, int stride,
        int pixelFormat, out IntPtr outData, out UIntPtr outSize, byte[] error)
    {
        bool lossless = ImageExportHelper.GetExportFormat(image, false).FileFormat == ImageFileFormat.Png;
        // The quality option only applies to lossy compression. For lossless compression, libwebp instead uses the
        // quality parameter as the compression effort (speed vs size), for which we use the default.
        var quality = lossless ? DEFAULT_QUALITY : GetQuality(options);
        var exif = image.HorizontalResolution > 0 && image.VerticalResolution > 0
            ? ExifHelper.CreateResolutionExif(image.HorizontalResolution, image.VerticalResolution)
            : null;
        return _compress(pixels, image.Width, image.Height, pixelFormat, quality, lossless ? 1 : 0, exif,
            new UIntPtr((uint) (exif?.Length ?? 0)), out outData, out outSize, error, error.Length);
    }

    protected override int Decompress(byte[] data, UIntPtr size, out IntPtr outPixels, out int outWidth,
        out int outHeight, out int outPixelFormat, out IntPtr outExif, out UIntPtr outExifSize, byte[] error) =>
        _decompress(data, size, out outPixels, out outWidth, out outHeight, out outPixelFormat, out outExif,
            out outExifSize, error, error.Length);
}
