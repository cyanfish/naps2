namespace NAPS2.Images.Bitwise;

public class CopyBitwiseImageOp : BinaryBitwiseImageOp
{
    // TODO: Consider requiring an explicit DiscardAlpha parameter
    
    public int DestXOffset { get; init; }

    public int DestYOffset { get; init; }

    protected override LockMode SrcLockMode => LockMode.ReadOnly;

    protected override LockMode DstLockMode => LockMode.WriteOnly;

    protected override void ValidateCore(PixelInfo src, PixelInfo dst)
    {
        if (dst.w - DestXOffset < src.w || dst.h - DestYOffset < src.h)
        {
            throw new ArgumentException();
        }
    }

    protected override void PerformCore(PixelInfo src, PixelInfo dst)
    {
        if (src.BitLayout == dst.BitLayout)
        {
            FastCopy(src, dst);
        }
        else if (src.bytesPerPixel is 3 or 4 && dst.bytesPerPixel is 3 or 4)
        {
            RgbaCopy(src, dst);
        }
        else if (src.bytesPerPixel is 3 or 4 && dst.bytesPerPixel == 1)
        {
            RgbToGrayCopy(src, dst);
        }
        // else if (src.bitsPerPixel == 1 && dst.bytesPerPixel is 3 or 4)
        // {
        //     BitToRgbaCopy(src, dst);
        // }
        // else if (dst.bitsPerPixel == 1 && src.bytesPerPixel is 3 or 4)
        // {
        //     RgbaToBitCopy(src, dst);
        // }
        else
        {
            throw new InvalidOperationException("Unsupported copy parameters");
        }
    }

    private unsafe void RgbaCopy(PixelInfo src, PixelInfo dst)
    {
        bool copyAlpha = src.bytesPerPixel == 4 && dst.bytesPerPixel == 4;
        bool copyToRgb = dst.bytesPerPixel >= 3;
        bool copyToGray = dst.bytesPerPixel == 1;
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset);
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + (j + DestXOffset) * dst.bytesPerPixel;
                var r = *(srcPixel + src.rOff);
                var g = *(srcPixel + src.gOff);
                var b = *(srcPixel + src.bOff);
                if (copyToRgb)
                {
                    *(dstPixel + dst.rOff) = r;
                    *(dstPixel + dst.gOff) = g;
                    *(dstPixel + dst.bOff) = b;
                }
                if (copyToGray)
                {
                    var luma = (byte) ((r * 299 + g * 587 + b * 114) / 1000);
                    *dstPixel = luma;
                }
                if (copyAlpha)
                {
                    *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
                }
            }
        }
    }

    // TODO: If branch prediction is actually so fast, can we combine some of these together?
    private unsafe void RgbToGrayCopy(PixelInfo src, PixelInfo dst)
    {
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset);
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + j + DestXOffset;
                var r = *(srcPixel + src.rOff);
                var g = *(srcPixel + src.gOff);
                var b = *(srcPixel + src.bOff);
                var luma = (byte) ((r * 299 + g * 587 + b * 114) / 1000);
                *dstPixel = luma;
            }
        }
    }

    private unsafe void FastCopy(PixelInfo src, PixelInfo dst)
    {
        var bytesPerRow = src.bytesPerPixel * src.w;
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset) + DestXOffset * dst.bytesPerPixel;
            Buffer.MemoryCopy(srcRow, dstRow, bytesPerRow, bytesPerRow);
        }
    }
}