using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

/// <summary>
/// An implementation of IMemoryImage that wraps a GDI+ image (System.Drawing.Bitmap).
/// </summary>
public class GdiImage : IMemoryImage
{
    public GdiImage(Bitmap bitmap)
    {
        Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
    }

    public Bitmap Bitmap { get; }

    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public float HorizontalResolution => Bitmap.HorizontalResolution;

    public float VerticalResolution => Bitmap.VerticalResolution;

    public void SetResolution(float xDpi, float yDpi) => Bitmap.SafeSetResolution(xDpi, yDpi);

    public ImagePixelFormat PixelFormat
    {
        get
        {
            switch (Bitmap.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return ImagePixelFormat.RGB24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return ImagePixelFormat.ARGB32;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return ImagePixelFormat.BW1;
                default:
                    return ImagePixelFormat.Unsupported;
            }
        }
    }

    public ImageFileFormat OriginalFileFormat => Bitmap.RawFormat.AsImageFileFormat();

    public ImageLockState Lock(LockMode lockMode, out IntPtr scan0, out int stride)
    {
        var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GetGdiLockMode(lockMode),
            Bitmap.PixelFormat);
        scan0 = bitmapData.Scan0;
        stride = Math.Abs(bitmapData.Stride);
        return new GdiImageLockState(Bitmap, bitmapData);
    }

    private ImageLockMode GetGdiLockMode(LockMode lockMode)
    {
        switch (lockMode)
        {
            case LockMode.ReadOnly:
                return ImageLockMode.ReadOnly;
            case LockMode.WriteOnly:
                return ImageLockMode.WriteOnly;
            default:
                return ImageLockMode.ReadWrite;
        }
    }

    public void Save(string path, ImageFileFormat imageFileFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFileFormat == ImageFileFormat.Unspecified)
        {
            imageFileFormat = GetFileFormatFromExtension(path);
        }
        if (imageFileFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            quality = Math.Max(Math.Min(quality, 100), 0);
            var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            Bitmap.Save(path, encoder, encoderParams);
        }
        else
        {
            Bitmap.Save(path, imageFileFormat.AsImageFormat());
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFileFormat, int quality = -1)
    {
        if (imageFileFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFileFormat));
        }
        if (imageFileFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            quality = Math.Max(Math.Min(quality, 100), 0);
            var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            Bitmap.Save(stream, encoder, encoderParams);
        }
        else
        {
            Bitmap.Save(stream, imageFileFormat.AsImageFormat());
        }
    }

    private ImageFileFormat GetFileFormatFromExtension(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => ImageFileFormat.Png,
            ".bmp" => ImageFileFormat.Bmp,
            ".jpg" or ".jpeg" => ImageFileFormat.Jpeg,
            _ => throw new ArgumentException($"Could not infer file format from extension: {path}")
        };
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }

    public IMemoryImage Clone()
    {
        return new GdiImage((Bitmap) Bitmap.Clone());
    }
}