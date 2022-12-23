namespace NAPS2.Images.Bitwise;

public class BrightnessBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly float _brightnessAdjusted;

    public BrightnessBitwiseImageOp(float brightness)
    {
        _brightnessAdjusted = brightness * 255;
    }

    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (data.bytesPerPixel is 3 or 4)
        {
            PerformRgba(data, partStart, partEnd);
        }
        else if (data.bytesPerPixel == 1)
        {
            PerformGray(data, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData data, int partStart, int partEnd)
    {
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j * data.bytesPerPixel;
                var r = *(pixel + data.rOff);
                var g = *(pixel + data.gOff);
                var b = *(pixel + data.bOff);

                var r2 = (int) (r + _brightnessAdjusted);
                var g2 = (int) (g + _brightnessAdjusted);
                var b2 = (int) (b + _brightnessAdjusted);

                r = (byte) (r2 < 0 ? 0 : r2 > 255 ? 255 : r2);
                g = (byte) (g2 < 0 ? 0 : g2 > 255 ? 255 : g2);
                b = (byte) (b2 < 0 ? 0 : b2 > 255 ? 255 : b2);

                *(pixel + data.rOff) = r;
                *(pixel + data.gOff) = g;
                *(pixel + data.bOff) = b;
            }
        }
    }

    private unsafe void PerformGray(BitwiseImageData data, int partStart, int partEnd)
    {
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            for (int j = 0; j < data.w; j++)
            {
                var pixel = row + j;
                var lum = (int) (*pixel + _brightnessAdjusted);
                *pixel = (byte) (lum < 0 ? 0 : lum > 255 ? 255 : lum);
            }
        }
    }
}