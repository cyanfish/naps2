using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace NAPS2.Images;

/// <summary>
/// The naps2_jpeg native library (from the NAPS2.Jpeg.Binaries package), which wraps jpegli/libwebp/libjxl.
/// </summary>
internal class JpegNativeLibrary : Unmanaged.NativeLibrary
{
    private static readonly Lazy<JpegNativeLibrary> LazyInstance = new(() =>
        new JpegNativeLibrary(FindPath() ?? throw new InvalidOperationException("Could not find naps2_jpeg")));

    public static JpegNativeLibrary Instance => LazyInstance.Value;

    public static string? FindPath()
    {
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_DEPS");
        try
        {
            var path = FindLibraryPath(PlatformCompat.System.JpegLibraryName, testRoot);
            return File.Exists(path) ? path : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static JpegNativeLibrary? TryGetInstance()
    {
        if (FindPath() == null)
        {
            return null;
        }
        try
        {
            return Instance;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public const int NAPS2_JPEG_PIXEL_RGB24 = 0;
    public const int NAPS2_JPEG_PIXEL_BGR24 = 1;
    public const int NAPS2_JPEG_PIXEL_RGBA32 = 2;
    public const int NAPS2_JPEG_PIXEL_BGRA32 = 3;
    public const int NAPS2_JPEG_PIXEL_GRAY8 = 4;

    public const int ERROR_SIZE = 256;

    private JpegNativeLibrary(string libraryPath)
        : base(libraryPath)
    {
        Compress = Load<naps2_jpeg_compress_delegate>();
        Free = Load<naps2_jpeg_free_delegate>();
    }

    public naps2_jpeg_compress_delegate Compress { get; }

    public naps2_jpeg_free_delegate Free { get; }

    public naps2_webp_compress_delegate WebpCompress => Load<naps2_webp_compress_delegate>();
    public naps2_webp_decompress_delegate WebpDecompress => Load<naps2_webp_decompress_delegate>();
    public naps2_jxl_compress_delegate JxlCompress => Load<naps2_jxl_compress_delegate>();
    public naps2_jxl_decompress_delegate JxlDecompress => Load<naps2_jxl_decompress_delegate>();
    public naps2_jxl_transcode_jpeg_delegate JxlTranscodeJpeg => Load<naps2_jxl_transcode_jpeg_delegate>();
    public naps2_icc_to_srgb_delegate IccToSrgb => Load<naps2_icc_to_srgb_delegate>();

    public static void ThrowIfError(int ret, byte[] error, string context)
    {
        if (ret != 0)
        {
            var message = Encoding.UTF8.GetString(error, 0, Math.Max(0, Array.IndexOf(error, (byte) 0)));
            throw new InvalidOperationException($"{context}: {message}");
        }
    }

    public unsafe void CopyNativeBufferToStreamAndFree(IntPtr data, UIntPtr size, Stream stream)
    {
        try
        {
            using var dataStream = new UnmanagedMemoryStream((byte*) data, (long) size.ToUInt64());
            dataStream.CopyTo(stream);
        }
        finally
        {
            Free(data);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_jpeg_compress_delegate(IntPtr pixels, int width, int height, int stride,
        int pixelFormat, int quality, double xDpi, double yDpi, out IntPtr outData, out UIntPtr outSize, byte[] error,
        int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void naps2_jpeg_free_delegate(IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_webp_compress_delegate(IntPtr pixels, int width, int height, int pixelFormat,
        int quality, int lossless, byte[]? exif, UIntPtr exifSize, out IntPtr outData, out UIntPtr outSize,
        byte[] error, int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_webp_decompress_delegate(byte[] data, UIntPtr size, out IntPtr outPixels,
        out int outWidth, out int outHeight, out int outPixelFormat, out IntPtr outExif, out UIntPtr outExifSize,
        byte[] error, int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_jxl_compress_delegate(IntPtr pixels, int width, int height, int pixelFormat,
        int quality, int lossless, byte[]? exif, UIntPtr exifSize, out IntPtr outData, out UIntPtr outSize,
        byte[] error, int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_jxl_transcode_jpeg_delegate(byte[] jpegData, UIntPtr jpegSize, byte[]? exif,
        UIntPtr exifSize, out IntPtr outData, out UIntPtr outSize, byte[] error, int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_jxl_decompress_delegate(byte[] data, UIntPtr size, out IntPtr outPixels,
        out int outWidth, out int outHeight, out int outPixelFormat, out IntPtr outExif, out UIntPtr outExifSize,
        byte[] error, int errorSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int naps2_icc_to_srgb_delegate(IntPtr pixels, int width, int height, int stride, int pixelFormat,
        byte[] icc, UIntPtr iccSize, byte[] error, int errorSize);
}
