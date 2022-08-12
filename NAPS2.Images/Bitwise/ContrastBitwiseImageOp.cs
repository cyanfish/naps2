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

    protected override void PerformCore(PixelInfo pix)
    {
        if (pix.bytesPerPixel is 3 or 4)
        {
            PerformRgba(pix);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(PixelInfo pix)
    {
        for (int i = 0; i < pix.h; i++)
        {
            var row = pix.data + pix.stride * i;
            for (int j = 0; j < pix.w; j++)
            {
                var pixel = row + j * pix.bytesPerPixel;
                var r = *(pixel + pix.rOff);
                var g = *(pixel + pix.gOff);
                var b = *(pixel + pix.bOff);

                int r2 = (int)(r * _contrastAdjusted + _offset);
                int g2 = (int)(g * _contrastAdjusted + _offset);
                int b2 = (int)(b * _contrastAdjusted + _offset);

                r = (byte)(r2 < 0 ? 0 : r2 > 255 ? 255 : r2);
                g = (byte)(g2 < 0 ? 0 : g2 > 255 ? 255 : g2);
                b = (byte)(b2 < 0 ? 0 : b2 > 255 ? 255 : b2);

                *(pixel + pix.rOff) = r;
                *(pixel + pix.gOff) = g;
                *(pixel + pix.bOff) = b;
            }
        }
    }
}