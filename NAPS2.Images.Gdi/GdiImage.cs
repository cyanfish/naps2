using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gdi;

/// <summary>
/// An implementation of IMemoryImage that wraps a GDI+ image (System.Drawing.Bitmap).
/// </summary>
public class GdiImage : IMemoryImage
{
    private ImageFileFormat? _originalFileFormat;
    
    public GdiImage(Bitmap bitmap)
    {
        Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
    }

    /// <summary>
    /// Gets the underlying System.Drawing.Bitmap object for this image.
    /// </summary>
    public Bitmap Bitmap { get; }

    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public float HorizontalResolution => Bitmap.HorizontalResolution;

    public float VerticalResolution => Bitmap.VerticalResolution;

    public void SetResolution(float xDpi, float yDpi) => Bitmap.SafeSetResolution(xDpi, yDpi);

    public ImagePixelFormat PixelFormat => Bitmap.PixelFormat.AsImagePixelFormat();

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        throw new NotImplementedException();
    }

    // TODO: Consider propagating this during transforms (when it makes sense); then maybe we can remove the "encodeOnce" check
    public ImageFileFormat OriginalFileFormat
    {
        get => _originalFileFormat ?? Bitmap.RawFormat.AsImageFileFormat();
        set => _originalFileFormat = value;
    }

    public ImageLockState Lock(LockMode lockMode, out IntPtr scan0, out int stride)
    {
        var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), lockMode.AsImageLockMode(),
            Bitmap.PixelFormat);
        scan0 = bitmapData.Scan0;
        stride = Math.Abs(bitmapData.Stride);
        return new GdiImageLockState(Bitmap, bitmapData);
    }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
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
        var newImage = new GdiImage((Bitmap) Bitmap.Clone());
        // TODO: We want to make original file format more consistent when copying around and transforming images 
        newImage._originalFileFormat = _originalFileFormat;
        return newImage;
    }

    public IMemoryImage SafeClone()
    {
        if (PixelFormat == ImagePixelFormat.BW1)
        {
            // TODO: This should do something better, but currently there are no use cases that need it
            // TODO: In general we might consider, rather than per-impl SafeClone, having an ImageContext method to do a binary copy with CreateImage + Lock
            return Clone();
        }
        var newBitmap = new Bitmap(Width, Height, PixelFormat.AsPixelFormat());
        using var g = Graphics.FromImage(newBitmap);
        if (PixelFormat == ImagePixelFormat.ARGB32)
        {
            g.Clear(Color.Transparent);
        }
        g.DrawImage(Bitmap, 0, 0, Width, Height);

        var newImage = new GdiImage(newBitmap);
        newImage.OriginalFileFormat = OriginalFileFormat;
        newImage.SetResolution(HorizontalResolution, VerticalResolution);
        return newImage;
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }
}