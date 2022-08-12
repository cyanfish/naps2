namespace NAPS2.Images.Bitwise;

public class BitPixelReader : IDisposable
{
    private const int THRESHOLD = 140 * 1000;

    private readonly ImageLockState _lock;
    private readonly PixelInfo _pix;
    private readonly bool _readRgb;

    public BitPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _pix);
        _readRgb = _pix.bytesPerPixel is 3 or 4;
    }

    public unsafe bool this[int y, int x]
    {
        get
        {
            if (_readRgb)
            {
                var pixel = _pix.data + _pix.stride * y + _pix.bytesPerPixel * x;
                var r = *(pixel + _pix.rOff);
                var g = *(pixel + _pix.gOff);
                var b = *(pixel + _pix.bOff);
                var luma = r * 299 + g * 587 + b * 114;
                return luma < THRESHOLD;
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