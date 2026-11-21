using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NAPS2.ImportExport;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images;

/// <summary>
/// Base class for codecs in the naps2_jpeg native library.
/// </summary>
internal abstract class AbstractBundledCodec : IBundledCodec
{
    protected const int DEFAULT_QUALITY = 75;

    protected static T? TryCreate<T>(Func<JpegNativeLibrary, T> create) where T : AbstractBundledCodec =>
        JpegNativeLibrary.TryGetInstance() is { } lib ? create(lib) : null;

    private readonly ImageFileFormat _format;
    private readonly bool _allowGray;

    protected AbstractBundledCodec(JpegNativeLibrary lib, ImageFileFormat format, bool allowGray)
    {
        Lib = lib;
        _format = format;
        _allowGray = allowGray;
    }

    protected JpegNativeLibrary Lib { get; }

    /// <summary>
    /// Gets the native pixel format if the encoder can read the image data directly, otherwise null (in which case
    /// the pixels are copied to a new buffer first).
    /// </summary>
    protected abstract int? GetDirectPixelFormat(BitwiseImageData data);

    protected abstract int Compress(IMemoryImage image, ImageSaveOptions options, IntPtr pixels, int stride,
        int pixelFormat, out IntPtr outData, out UIntPtr outSize, byte[] error);

    protected virtual int Decompress(byte[] data, UIntPtr size, out IntPtr outPixels, out int outWidth,
        out int outHeight, out int outPixelFormat, out IntPtr outExif, out UIntPtr outExifSize, byte[] error) =>
        throw new NotSupportedException();

    protected static int GetQuality(ImageSaveOptions options) =>
        options.Quality == -1 ? DEFAULT_QUALITY : options.Quality.Clamp(1, 100);

    public unsafe bool TryEncode(IMemoryImage image, Stream stream, ImageSaveOptions options)
    {
        using var helper = PixelFormatHelper.Create(image,
            minFormat: _allowGray ? ImagePixelFormat.Gray8 : ImagePixelFormat.RGB24);
        var error = new byte[JpegNativeLibrary.ERROR_SIZE];
        IntPtr outData;
        UIntPtr outSize;
        BitwiseImageData data;
        using (helper.Image.Lock(LockMode.ReadOnly, out data))
        {
            if (GetDirectPixelFormat(data) is { } directPixelFormat)
            {
                JpegNativeLibrary.ThrowIfError(
                    Compress(image, options, data.safePtr, data.stride, directPixelFormat, out outData, out outSize,
                        error), error, $"Error encoding {_format}");
                Lib.CopyNativeBufferToStreamAndFree(outData, outSize, stream);
                return true;
            }
        }
        var (subPixelType, pixelFormat) = data switch
        {
            { hasAlpha: true } => (SubPixelType.Rgba, JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGBA32),
            { bytesPerPixel: <= 1 } => (SubPixelType.Gray, JpegNativeLibrary.NAPS2_JPEG_PIXEL_GRAY8),
            _ => (SubPixelType.Rgb, JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGB24)
        };
        var pixelInfo = new PixelInfo(image.Width, image.Height, subPixelType);
        var pixels = new byte[pixelInfo.Length];
        new CopyBitwiseImageOp().Perform(helper.Image, pixels, pixelInfo);
        fixed (byte* ptr = pixels)
        {
            JpegNativeLibrary.ThrowIfError(
                Compress(image, options, (IntPtr) ptr, pixelInfo.Stride, pixelFormat, out outData, out outSize,
                    error), error, $"Error encoding {_format}");
        }
        Lib.CopyNativeBufferToStreamAndFree(outData, outSize, stream);
        return true;
    }

    public bool TryDecode(ImageContext imageContext, Stream stream, [NotNullWhen(true)] out IMemoryImage? image)
    {
        var input = new MemoryStream();
        stream.CopyTo(input);
        var error = new byte[JpegNativeLibrary.ERROR_SIZE];
        JpegNativeLibrary.ThrowIfError(
            Decompress(input.GetBuffer(), new UIntPtr((uint) input.Length), out var pixels, out var width,
                out var height, out var pixelFormat, out var exifData, out var exifSize, error),
            error, $"Error decoding {_format}");
        try
        {
            var (imagePixelFormat, subPixelType) = pixelFormat switch
            {
                JpegNativeLibrary.NAPS2_JPEG_PIXEL_GRAY8 => (ImagePixelFormat.Gray8, SubPixelType.Gray),
                JpegNativeLibrary.NAPS2_JPEG_PIXEL_RGB24 => (ImagePixelFormat.RGB24, SubPixelType.Rgb),
                _ => (ImagePixelFormat.ARGB32, SubPixelType.Rgba)
            };
            image = imageContext.Create(width, height, imagePixelFormat);
            new CopyBitwiseImageOp().Perform(pixels, new PixelInfo(width, height, subPixelType), image);
            image.OriginalFileFormat = _format;
            if (exifData != IntPtr.Zero)
            {
                var exif = new byte[(int) exifSize.ToUInt32()];
                Marshal.Copy(exifData, exif, 0, exif.Length);
                if (ExifHelper.ReadResolution(exif) is var (xDpi, yDpi))
                {
                    image.SetResolution((float) xDpi, (float) yDpi);
                }
            }
            return true;
        }
        finally
        {
            Lib.Free(pixels);
            Lib.Free(exifData);
        }
    }
}
