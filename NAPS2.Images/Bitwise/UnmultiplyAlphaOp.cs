namespace NAPS2.Images.Bitwise;

internal class UnmultiplyAlphaOp : UnaryBitwiseImageOp
{
    protected override void PerformCore(BitwiseImageData data, int partStart, int partEnd)
    {
        if (data.bytesPerPixel == 4)
        {
            PerformRgba(data, partStart, partEnd);
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
                var a = *(pixel + data.aOff);

                if (a != 255)
                {
                    var mul = 255 / (float) a;
                    var r2 = (int) Math.Round(r * mul);
                    var g2 = (int) Math.Round(g * mul);
                    var b2 = (int) Math.Round(b * mul);

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
}