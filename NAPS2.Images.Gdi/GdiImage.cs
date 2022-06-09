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
        var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GetGdiLockMode(lockMode), Bitmap.PixelFormat);
        scan0 = bitmapData.Scan0;
        stride = Math.Abs(bitmapData.Stride);
        return new GdiImageLockState(bitmapData);
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

    public void Unlock(ImageLockState state)
    {
        Bitmap.UnlockBits(state.AsBitmapData());
    }

    public void Save(string path, ImageFileFormat imageFileFormat = ImageFileFormat.Unspecified)
    {
        if (imageFileFormat == ImageFileFormat.Unspecified)
        {
            Bitmap.Save(path);
        }
        else
        {
            Bitmap.Save(path, imageFileFormat.AsImageFormat());
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFileFormat)
    {
        if (imageFileFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFileFormat));
        }
        Bitmap.Save(stream, imageFileFormat.AsImageFormat());
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }

    public IMemoryImage Clone()
    {
        return new GdiImage((Bitmap)Bitmap.Clone());
    }
}