using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gdi;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class GdiImageLockState : ImageLockState
{
    public static GdiImageLockState Create(Bitmap bitmap, LockMode lockMode, out BitwiseImageData imageData)
    {
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), lockMode.AsImageLockMode(),
            bitmap.PixelFormat);
        var subPixelType = bitmap.PixelFormat switch
        {
            PixelFormat.Format32bppArgb => SubPixelType.Bgra,
            PixelFormat.Format24bppRgb => SubPixelType.Bgr,
            PixelFormat.Format8bppIndexed => SubPixelType.Gray,
            PixelFormat.Format1bppIndexed => SubPixelType.Bit,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData(bitmapData.Scan0,
            new PixelInfo(bitmap.Width, bitmap.Height, subPixelType, bitmapData.Stride));
        return new GdiImageLockState(bitmap, bitmapData);
    }

    private readonly Bitmap _bitmap;
    private readonly BitmapData _bitmapData;
    private bool _disposed;

    private GdiImageLockState(Bitmap bitmap, BitmapData bitmapData)
    {
        _bitmap = bitmap;
        _bitmapData = bitmapData;
    }

    public override void Dispose()
    {
        lock (this)
        {
            if (_disposed) return;
            _disposed = true;
        }
        _bitmap.UnlockBits(_bitmapData);
    }
}