namespace NAPS2.Images.Bitwise;

public class BitPixelReader : IDisposable
{
    private const int THRESHOLD = 140 * 1000;

    private readonly ImageLockState _lock;
    private readonly BitwiseImageData _data;
    private readonly bool _readRgb;
    private readonly bool _readGray;
    private readonly bool _readBit;

    public BitPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _data);
        _readRgb = _data.bytesPerPixel is 3 or 4;
        _readGray = _data.bytesPerPixel == 1;
        _readBit = _data.bitsPerPixel == 1;
    }

    public unsafe bool this[int y, int x]
    {
        get
        {
            if (_readRgb)
            {
                var pixel = _data.ptr + _data.stride * y + _data.bytesPerPixel * x;
                var r = *(pixel + _data.rOff);
                var g = *(pixel + _data.gOff);
                var b = *(pixel + _data.bOff);
                var luma = r * 299 + g * 587 + b * 114;
                return luma < THRESHOLD;
            }
            if (_readGray)
            {
                var pixel = _data.ptr + _data.stride * y + _data.bytesPerPixel * x;
                var luma = *pixel * 1000;
                return luma < THRESHOLD;
            }
            if (_readBit)
            {
                var pixel = _data.ptr + _data.stride * y + x / 8;
                var bit = (*pixel >> (7 - x % 8)) & 1;
                return bit == 1;
            }
            throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}