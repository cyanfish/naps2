namespace NAPS2.Images.Bitwise;

public class SaturationBitwiseImageOp : UnaryBitwiseImageOp
{
    private readonly float _saturationAdjusted;

    public SaturationBitwiseImageOp(float saturation)
    {
        _saturationAdjusted = saturation + 1;
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
                var r = *(pixel + data.rOff) / 255f;
                var g = *(pixel + data.gOff) / 255f;
                var b = *(pixel + data.bOff) / 255f;
                
                // TODO: This is a bit different than HueShiftBitwiseImageOp (HSL instead of HSV & floats instead of bytes), can we normalize?
                // Convert RGB to HSL

                var max = Math.Max(r, Math.Max(g, b));
                var min = Math.Min(r, Math.Min(g, b));
                if (max == min)
                {
                    continue;
                }

                float h = 0.0f;
                float delta = max - min;
                if (r == max)
                {
                    h = (g - b) / delta;
                    if (h < 0)
                    {
                        h += 6;
                    }
                }
                else if (g == max)
                {
                    h = 2 + (b - r) / delta;
                }
                else if (b == max)
                {
                    h = 4 + (r - g) / delta;
                }

                var l = (max + min) / 2;
                var p = 1 - Math.Abs(2 * l - 1);
                var s = delta / p;

                // Adjust saturation

                s = Math.Min(s * _saturationAdjusted, 1);

                // Convert HSL to RGB

                var c = p * s;
                var x = c * (1 - Math.Abs(h % 2 - 1));
                var m = l - c / 2;

                var sextant = (int) h;
                switch (sextant)
                {
                    case 0:
                        r = c;
                        g = x;
                        b = 0;
                        break;
                    case 1:
                        r = x;
                        g = c;
                        b = 0;
                        break;
                    case 2:
                        r = 0;
                        g = c;
                        b = x;
                        break;
                    case 3:
                        r = 0;
                        g = x;
                        b = c;
                        break;
                    case 4:
                        r = x;
                        g = 0;
                        b = c;
                        break;
                    default:
                        r = c;
                        g = 0;
                        b = x;
                        break;
                }

                *(pixel + data.rOff) = (byte) Math.Round((r + m) * 255);
                *(pixel + data.gOff) = (byte) Math.Round((g + m) * 255);
                *(pixel + data.bOff) = (byte) Math.Round((b + m) * 255);
            }
        }
    }
}