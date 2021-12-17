using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Storage;

public class GdiImage : IImage
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

    public void SetResolution(float xDpi, float yDpi)
    {
        if (xDpi > 0 && yDpi > 0)
        {
            Bitmap.SetResolution(xDpi, yDpi);
        }
    }

    public StoragePixelFormat PixelFormat
    {
        get
        {
            switch (Bitmap.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return StoragePixelFormat.RGB24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return StoragePixelFormat.ARGB32;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return StoragePixelFormat.BW1;
                default:
                    return StoragePixelFormat.Unsupported;
            }
        }
    }

    public bool IsOriginalLossless => Equals(Bitmap.RawFormat, ImageFormat.Bmp) || Equals(Bitmap.RawFormat, ImageFormat.Png);

    public object Lock(LockMode lockMode, out IntPtr scan0, out int stride)
    {
        var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), GetGdiLockMode(lockMode), Bitmap.PixelFormat);
        scan0 = bitmapData.Scan0;
        stride = Math.Abs(bitmapData.Stride);
        return bitmapData;
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

    public void Unlock(object state)
    {
        var bitmapData = (BitmapData)state;
        Bitmap.UnlockBits(bitmapData);
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }

    public IImage Clone()
    {
        return new GdiImage((Bitmap)Bitmap.Clone());
    }
}