namespace NAPS2.Images.Bitwise;

public class RmseBitwiseImageOp : BinaryBitwiseImageOp
{
    protected override LockMode SrcLockMode => LockMode.ReadOnly;
    protected override LockMode DstLockMode => LockMode.ReadOnly;

    private long _count;
    private long _total;

    public double Rmse { get; private set; }

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
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
        long partCount = 0;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstRow = dst.ptr + dst.stride * i;
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + j * dst.bytesPerPixel;

                int r1 = *(srcPixel + src.rOff);
                int g1 = *(srcPixel + src.gOff);
                int b1 = *(srcPixel + src.bOff);

                int r2 = *(dstPixel + src.rOff);
                int g2 = *(dstPixel + src.gOff);
                int b2 = *(dstPixel + src.bOff);

                partCount += (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);

                byte a1 = 255, a2 = 255;
                if (src.hasAlpha)
                {
                    a1 = *(srcPixel + src.aOff);
                }
                if (dst.hasAlpha)
                {
                    a2 = *(dstPixel + dst.aOff);
                }
                partCount += (a1 - a2) * (a1 - a2);
            }
        }
        lock (this)
        {
            _count += partCount;
            _total += src.w * (partEnd - partStart) * (src.hasAlpha || dst.hasAlpha ? 4 : 3);
        }
    }

    protected override void FinishCore()
    {
        Rmse = Math.Sqrt(_count / (double) _total);
    }
}