using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public class GdiImageLockState : ImageLockState
{
    private readonly Bitmap _bitmap;
    private readonly BitmapData _bitmapData;
    private bool _disposed;

    public GdiImageLockState(Bitmap bitmap, BitmapData bitmapData)
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
