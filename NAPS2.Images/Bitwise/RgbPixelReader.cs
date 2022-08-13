namespace NAPS2.Images.Bitwise;

public class RgbPixelReader : IDisposable
{
    private readonly ImageLockState _lock;
    private readonly PixelInfo _pix;
    private readonly bool _readRgb;
    private readonly bool _readGray;
    private readonly bool _readBit;

    public RgbPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _pix);
        _readRgb = _pix.bytesPerPixel is 3 or 4;
        _readGray = _pix.bytesPerPixel == 1;
        _readBit = _pix.bitsPerPixel == 1;
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
            if (_readGray)
            {
                var pixel = _pix.data + _pix.stride * y + _pix.bytesPerPixel * x;
                var luma = *pixel;
                return (luma, luma, luma);
            }
            if (_readBit)
            {
                var monoByte = *(_pix.data + _pix.stride * y + x / 8);
                var bit = (monoByte >> (7 - x % 8)) & 1;
                return bit == 0 ? (0, 0, 0) : (255, 255, 255);
            }
            throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}