namespace NAPS2.Images.Bitwise;

public class RgbPixelReader : IDisposable
{
    private readonly ImageLockState _lock;
    private readonly PixelInfo _pix;
    private readonly bool _readRgb;

    public RgbPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _pix);
        _readRgb = _pix.bytesPerPixel is 3 or 4;
    }

    public unsafe (int r, int g, int b) this[int y, int x]
    {
        get
        {
            if (_readRgb)
            {
                var pixel = _pix.data + _pix.stride * y + _pix.bytesPerPixel * x;
                var r = *(pixel + _pix.rOff);
                var g = *(pixel + _pix.gOff);
                var b = *(pixel + _pix.bOff);
                return (r, g, b);
            }
            // TODO: 1bpp
            throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}