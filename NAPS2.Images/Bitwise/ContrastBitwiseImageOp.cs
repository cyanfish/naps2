namespace NAPS2.Images.Bitwise;

public class ContrastBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly float _contrastAdjusted;
    private readonly float _offset;

    public ContrastBitwiseImageOp(float contrast)
    {
        // convert +/-1 input range to a logarithmic scaled multiplier
        _contrastAdjusted = (float)Math.Pow(2.718281, contrast * 2);
        // see http://docs.rainmeter.net/tips/colormatrix-guide/ for offset & matrix calculation
        _offset = (1 - _contrastAdjusted) / 2 * 255;
    }

    protected override void PerformCore(BitwiseImageData data)
    {
        if (data.bytesPerPixel is 3 or 4)
        {
            PerformRgba(data);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData data)
    {
        for (int i = 0; i < data.h; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                var r = *(pixel + data.rOff);
                var g = *(pixel + data.gOff);
                var b = *(pixel + data.bOff);

                int r2 = (int)(r * _contrastAdjusted + _offset);
                int g2 = (int)(g * _contrastAdjusted + _offset);
                int b2 = (int)(b * _contrastAdjusted + _offset);

                r = (byte)(r2 < 0 ? 0 : r2 > 255 ? 255 : r2);
                g = (byte)(g2 < 0 ? 0 : g2 > 255 ? 255 : g2);
                b = (byte)(b2 < 0 ? 0 : b2 > 255 ? 255 : b2);

                *(pixel + data.rOff) = r;
                *(pixel + data.gOff) = g;
                *(pixel + data.bOff) = b;
            }
        }
    }
}