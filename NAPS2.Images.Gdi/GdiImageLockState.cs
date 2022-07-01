using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public class GdiImageLockState : ImageLockState
{
    private readonly Bitmap _bitmap;
    private readonly BitmapData _bitmapData;

    public GdiImageLockState(Bitmap bitmap, BitmapData bitmapData)
    {
        _bitmap = bitmap;
        _bitmapData = bitmapData;
    }

    public override void Dispose()
    {
        _bitmap.UnlockBits(_bitmapData);
    }
}
