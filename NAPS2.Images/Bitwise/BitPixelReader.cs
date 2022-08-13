namespace NAPS2.Images.Bitwise;

public class BitPixelReader : IDisposable
{
    private const int THRESHOLD = 140 * 1000;

    private readonly ImageLockState _lock;
    private readonly BitwiseImageData _data;
    private readonly bool _readRgb;

    public BitPixelReader(IMemoryImage image)
    {
        _lock = image.Lock(LockMode.ReadOnly, out _data);
        _readRgb = _data.bytesPerPixel is 3 or 4;
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
            // TODO: 1bpp
            throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}