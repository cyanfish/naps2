namespace NAPS2.Images.Bitwise;

// TODO: experimental
public class BilateralFilterOp : BinaryBitwiseImageOp
{
    public BilateralFilterOp()
    {
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
        bool copyAlpha = src.hasAlpha && dst.hasAlpha;
        const int filterSize = 9;
        const int s = filterSize / 2;

        var filter = new int[filterSize, filterSize];
        for (int filterX = 0; filterX < filterSize; filterX++)
        {
            for (int filterY = 0; filterY < filterSize; filterY++)
            {
                int dx = filterX - s;
                int dy = filterY - s;
                var dmax = Math.Sqrt(2 * s * s);
                var d = Math.Sqrt(dx * dx + dy * dy) / dmax;
                filter[filterX, filterY] = (int)((1 - d) * 256);
            }
        }

        var diffWeights = new int[256];
        for (int i = 0; i < 32; i++)
        {
            diffWeights[i] = 32 - i;
        }

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
                    int rTotal = 0, gTotal = 0, bTotal = 0;
                    int weightTotal = 0;
                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = j - s + filterX;
                            int imageY = i - s + filterY;

                            var pixel = src.ptr + src.stride * imageY + src.bytesPerPixel * imageX;

                            var r2 = *(pixel + src.rOff);
                            var g2 = *(pixel + src.gOff);
                            var b2 = *(pixel + src.bOff);

                            // TODO: Better color distance
                            var diff = Math.Abs((r + g + b) / 3 - (r2 + g2 + b2) / 3);
                            var weight = filter[filterX, filterY] * diffWeights[diff];
                            weightTotal += weight;
                            rTotal += r2 * weight;
                            gTotal += g2 * weight;
                            bTotal += b2 * weight;
                        }
                    }
                    r = rTotal / weightTotal;
                    g = gTotal / weightTotal;
                    b = bTotal / weightTotal;
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