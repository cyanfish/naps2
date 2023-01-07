using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gdi;

/// <summary>
/// An implementation of IMemoryImage that wraps a GDI+ image (System.Drawing.Bitmap).
/// </summary>
#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
public class GdiImage : IMemoryImage
{
    public GdiImage(ImageContext imageContext, Bitmap bitmap)
    {
        if (imageContext is not GdiImageContext) throw new ArgumentException("Expected GdiImageContext");
        ImageContext = imageContext;
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap));
        }
        FixedPixelFormat = GdiPixelFormatFixer.MaybeFixPixelFormat(ref bitmap);
        Bitmap = bitmap;
        OriginalFileFormat = bitmap.RawFormat.AsImageFileFormat();
        LogicalPixelFormat = PixelFormat;
    }

    public ImageContext ImageContext { get; }

    /// <summary>
    /// Gets the underlying System.Drawing.Bitmap object for this image.
    /// </summary>
    public Bitmap Bitmap { get; }

    internal bool FixedPixelFormat { get; }

    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public float HorizontalResolution => Bitmap.HorizontalResolution;

    public float VerticalResolution => Bitmap.VerticalResolution;

    public void SetResolution(float xDpi, float yDpi) => Bitmap.SafeSetResolution(xDpi, yDpi);

    public ImagePixelFormat PixelFormat => Bitmap.PixelFormat.AsImagePixelFormat();

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        return GdiImageLockState.Create(Bitmap, lockMode, out imageData);
    }

    // TODO: Consider propagating this during transforms (when it makes sense); then maybe we can remove the "encodeOnce" check
    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        if (imageFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            var (encoder, encoderParams) = GetJpegSaveArgs(quality);
            Bitmap.Save(path, encoder, encoderParams);
        }
        else
        {
            Bitmap.Save(path, imageFormat.AsImageFormat());
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        if (imageFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            var (encoder, encoderParams) = GetJpegSaveArgs(quality);
            Bitmap.Save(stream, encoder, encoderParams);
        }
        else
        {
            Bitmap.Save(stream, imageFormat.AsImageFormat());
        }
    }

    private static (ImageCodecInfo, EncoderParameters) GetJpegSaveArgs(int quality)
    {
        quality = Math.Max(Math.Min(quality, 100), 0);
        var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        return (encoder, encoderParams);
    }

    public IMemoryImage Clone()
    {
        // TODO: Ideally we'd like to make use of copy-on-write. But GDI copy-on-write (Clone) is not thread safe.
        // Maybe we can implement something like CopyOnWrite<Bitmap> to use instead of just Bitmap.
        return this.Copy();
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }
}