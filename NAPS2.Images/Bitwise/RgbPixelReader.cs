namespace NAPS2.Images.Bitwise;

public class RgbPixelReader : IDisposable
{
    private readonly ImageLockState _lock;
    private readonly BitwiseImageData _data;
    private readonly bool _readRgb;
    private readonly bool _readGray;
    private readonly bool _readBit;

    public RgbPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _data);
        _readRgb = _data.bytesPerPixel is 3 or 4;
        _readGray = _data.bytesPerPixel == 1;
        _readBit = _data.bitsPerPixel == 1;
    }

    public unsafe (int r, int g, int b) this[int y, int x]
    {
        get
        {
            if (_readRgb)
            {
                var pixel = _data.ptr + _data.stride * y + _data.bytesPerPixel * x;
                var r = *(pixel + _data.rOff);
                var g = *(pixel + _data.gOff);
                var b = *(pixel + _data.bOff);
                return (r, g, b);
            }
            if (_readGray)
            {
                var pixel = _data.ptr + _data.stride * y + _data.bytesPerPixel * x;
                var luma = *pixel;
                return (luma, luma, luma);
            }
            if (_readBit)
            {
                var monoByte = *(_data.ptr + _data.stride * y + x / 8);
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