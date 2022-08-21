namespace NAPS2.Images.Bitwise;

public class SharpenBitwiseImageOp : BinaryBitwiseImageOp
{
    private readonly float _sharpness;

    public SharpenBitwiseImageOp(float sharpness)
    {
        _sharpness = sharpness;
    }

    protected override LockMode SrcLockMode => LockMode.ReadOnly;
    protected override LockMode DstLockMode => LockMode.WriteOnly;

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        // TODO: Implement grayscale?
        if (src.bytesPerPixel is 3 or 4 && dst.bytesPerPixel is 3 or 4)
        {
            PerformRgba(src, dst, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        // TODO: This is super slow (as we read each pixel 25 times), I wonder if there's a good way to optimize?
        bool copyAlpha = src.bytesPerPixel == 4 && dst.bytesPerPixel == 4;
        const int filterSize = 5;
        const int s = filterSize / 2;

        var filter = new [,]
        {
            { -1, -1, -1, -1, -1 },
            { -1, 2, 2, 2, -1 },
            { -1, 2, 16, 2, -1 },
            { -1, 2, 2, 2, -1 },
            { -1, -1, -1, -1, -1 }
        };
        double bias = 1.0 - _sharpness;
        double factor = _sharpness / 16.0;

        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstRow = dst.ptr + dst.stride * i;
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + j * dst.bytesPerPixel;
                int r = *(srcPixel + src.rOff);
                int g = *(srcPixel + src.gOff);
                int b = *(srcPixel + src.bOff);

                if (j > s && j < src.w - s && i > s && i < src.h - s)
                {
                    int rMult = 0, gMult = 0, bMult = 0;
                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = j - s + filterX;
                            int imageY = i - s + filterY;

                            var pixel = src.ptr + src.stride * imageY + src.bytesPerPixel * imageX;

                            rMult += *(pixel + src.rOff) * filter[filterX, filterY];
                            gMult += *(pixel + src.gOff) * filter[filterX, filterY];
                            bMult += *(pixel + src.bOff) * filter[filterX, filterY];
                        }
                    }
                    r = Math.Min(Math.Max((int) (factor * rMult + bias * r), 0), 255);
                    g = Math.Min(Math.Max((int) (factor * gMult + bias * g), 0), 255);
                    b = Math.Min(Math.Max((int) (factor * bMult + bias * b), 0), 255);
                }
                *(dstPixel + dst.rOff) = (byte) r;
                *(dstPixel + dst.gOff) = (byte) g;
                *(dstPixel + dst.bOff) = (byte) b;
                if (copyAlpha)
                {
                    *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
                }
            }
        }
    }
}