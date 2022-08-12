namespace NAPS2.Images.Bitwise;

public class BrightnessBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly float _brightnessAdjusted;

    public BrightnessBitwiseImageOp(float brightness)
    {
        _brightnessAdjusted = brightness * 255;
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
                
                var r2 = (int)(r + _brightnessAdjusted);
                var g2 = (int)(g + _brightnessAdjusted);
                var b2 = (int)(b + _brightnessAdjusted);

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