namespace NAPS2.Images.Bitwise;

public class HueShiftBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly float _shiftAdjusted;

    public HueShiftBitwiseImageOp(float shift)
    {
        _shiftAdjusted = shift * -3;
        if (_shiftAdjusted < 0)
        {
            _shiftAdjusted += 6;
        }
    }

    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (data.bytesPerPixel is 3 or 4)
        {
            PerformRgba(data, partStart, partEnd);
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

                int max = Math.Max(r, Math.Max(g, b));
                int min = Math.Min(r, Math.Min(g, b));

                if (max == min)
                {
                    continue;
                }

                float hue = 0.0f;
                float delta = max - min;
                if (r == max)
                {
                    hue = (g - b) / delta;
                }
                else if (g == max)
                {
                    hue = 2 + (b - r) / delta;
                }
                else if (b == max)
                {
                    hue = 4 + (r - g) / delta;
                }
                hue += _shiftAdjusted;
                hue = (hue + 6) % 6;

                float sat = (max == 0) ? 0 : 1f - (1f * min / max);
                float val = max;

                int hi = (int) Math.Floor(hue);
                float f = hue - hi;

                byte v = (byte) (val);
                byte p = (byte) (val * (1 - sat));
                byte q = (byte) (val * (1 - f * sat));
                byte t = (byte) (val * (1 - (1 - f) * sat));

                if (hi == 0)
                {
                    r = v;
                    g = t;
                    b = p;
                }
                else if (hi == 1)
                {
                    r = q;
                    g = v;
                    b = p;
                }
                else if (hi == 2)
                {
                    r = p;
                    g = v;
                    b = t;
                }
                else if (hi == 3)
                {
                    r = p;
                    g = q;
                    b = v;
                }
                else if (hi == 4)
                {
                    r = t;
                    g = p;
                    b = v;
                }
                else
                {
                    r = v;
                    g = p;
                    b = q;
                }

                *(pixel + data.rOff) = r;
                *(pixel + data.gOff) = g;
                *(pixel + data.bOff) = b;
            }
        }
    }
}