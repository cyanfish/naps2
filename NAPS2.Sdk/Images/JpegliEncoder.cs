using System.Text;

namespace NAPS2.Images;

/// <summary>
/// Encodes JPEG images with jpegli (via the naps2_jpeg native library), which produces significantly smaller files
/// than the platform encoders at the same quality.
/// </summary>
internal class JpegliEncoder : IBundledEncoder
{
    private const int DEFAULT_QUALITY = 75;
    private const int ERROR_BUFFER_SIZE = 256;

    /// <summary>
    /// Creates an encoder if the naps2_jpeg native library is present and can be loaded, otherwise returns null.
    /// </summary>
    public static JpegliEncoder? TryCreate()
    {
        if (JpegNativeLibrary.FindPath() == null)
        {
            return null;
        }
        try
        {
            return new JpegliEncoder(JpegNativeLibrary.Instance);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private readonly JpegNativeLibrary _lib;

    private JpegliEncoder(JpegNativeLibrary lib)
    {
        _lib = lib;
    }

    public bool TryEncode(IMemoryImage image, Stream stream, ImageSaveOptions options)
    {
        var quality = options.Quality == -1 ? DEFAULT_QUALITY : options.Quality.Clamp(1, 100);
        // JPEGs are always encoded with 3 channels (grayscale images are expanded to RGB) as grayscale JPEGs can't be
        // embedded directly in PDFs. Any alpha channel is ignored.
        using var helper = PixelFormatHelper.Create(image, minFormat: ImagePixelFormat.RGB24);
        using var lockState = helper.Image.Lock(LockMode.ReadOnly, out var data);
        var pixelFormat = GetPixelFormat(data);
        if (pixelFormat == null)
        {
            // Unsupported pixel layout, use the platform encoder instead
            return false;
        }
        var error = new byte[ERROR_BUFFER_SIZE];
        var ret = _lib.Compress(data.safePtr, data.w, data.h, data.stride, pixelFormat.Value, quality,
            image.HorizontalResolution, image.VerticalResolution, out var outData, out var outSize, error,
            error.Length);
        if (ret != 0)
        {
            var message = Encoding.UTF8.GetString(error, 0, Math.Max(0, Array.IndexOf(error, (byte) 0)));
            throw new InvalidOperationException($"Error encoding JPEG: {message}");
        }
        try
        {
            unsafe
            {
                using var outStream = new UnmanagedMemoryStream((byte*) outData, (long) outSize.ToUInt64());
                outStream.CopyTo(stream);
            }
        }
        finally
        {
            _lib.Free(outData);
        }
        return true;
    }

    private static int? GetPixelFormat(BitwiseImageData data)
    {
        if (data.invertY || data.invertColorSpace || data.stride <= 0)
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
}
