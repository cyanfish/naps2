using NAPS2.ImportExport;

namespace NAPS2.Images;

/// <summary>
/// Encodes and decodes JPEG XL images with libjxl (via the naps2_jpeg native library).
/// </summary>
internal class JpegXlCodec : AbstractBundledCodec
{
    public static JpegXlCodec? TryCreate() => TryCreate(lib => new JpegXlCodec(lib));

    private readonly JpegNativeLibrary.naps2_jxl_compress_delegate _compress;
    private readonly JpegNativeLibrary.naps2_jxl_decompress_delegate _decompress;
    private readonly JpegNativeLibrary.naps2_jxl_transcode_jpeg_delegate _transcodeJpeg;

    private JpegXlCodec(JpegNativeLibrary lib)
        : base(lib, ImageFileFormat.JpegXl, allowGray: true)
    {
        _compress = lib.JxlCompress;
        _decompress = lib.JxlDecompress;
        _transcodeJpeg = lib.JxlTranscodeJpeg;
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
        var exif = image.HorizontalResolution > 0 && image.VerticalResolution > 0
            ? ExifHelper.CreateResolutionExif(image.HorizontalResolution, image.VerticalResolution)
            : null;
        return _compress(pixels, image.Width, image.Height, pixelFormat, GetQuality(options), lossless ? 1 : 0, exif,
            new UIntPtr((uint) (exif?.Length ?? 0)), out outData, out outSize, error, error.Length);
    }

    /// <summary>
    /// Losslessly recompresses a JPEG file as JPEG XL, instead of re-encoding the pixels (which would lose more
    /// quality). The original JPEG file can be reconstructed exactly from the result.
    /// </summary>
    public bool TryTranscodeJpeg(string jpegPath, string path)
    {
        var jpegData = File.ReadAllBytes(jpegPath);
        var stream = new MemoryStream();
        try
        {
            DoTranscodeJpeg(jpegData, stream);
        }
        catch (InvalidOperationException)
        {
            // Some JPEGs can't be losslessly recompressed (e.g. with arithmetic coding), so re-encode them instead
            return false;
        }
        File.WriteAllBytes(path, stream.ToArray());
        return true;
    }

    private void DoTranscodeJpeg(byte[] jpegData, Stream stream)
    {
        // Any Exif metadata in the JPEG is kept, otherwise the resolution is added (as JPEG XL readers don't
        // generally read it from the JFIF header stored for reconstructing the JPEG)
        var header = JpegFormatHelper.ReadHeader(new MemoryStream(jpegData));
        var exif = header is { HasExifHeader: false, HorizontalDpi: > 0, VerticalDpi: > 0 }
            ? ExifHelper.CreateResolutionExif(header.HorizontalDpi, header.VerticalDpi)
            : null;
        var error = new byte[JpegNativeLibrary.ERROR_SIZE];
        JpegNativeLibrary.ThrowIfError(
            _transcodeJpeg(jpegData, new UIntPtr((uint) jpegData.Length), exif, new UIntPtr((uint) (exif?.Length ?? 0)),
                out var outData, out var outSize, error, error.Length),
            error, "Error recompressing JPEG as JPEG XL");
        Lib.CopyNativeBufferToStreamAndFree(outData, outSize, stream);
    }

    protected override int Decompress(byte[] data, UIntPtr size, out IntPtr outPixels, out int outWidth,
        out int outHeight, out int outPixelFormat, out IntPtr outExif, out UIntPtr outExifSize, byte[] error) =>
        _decompress(data, size, out outPixels, out outWidth, out outHeight, out outPixelFormat, out outExif,
            out outExifSize, error, error.Length);
}
