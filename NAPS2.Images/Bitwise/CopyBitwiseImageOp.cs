namespace NAPS2.Images.Bitwise;

// TODO: Need to double check callers set resolution when needed
public class CopyBitwiseImageOp : BinaryBitwiseImageOp
{
    private const int R_MULT = 299;
    private const int G_MULT = 587;
    private const int B_MULT = 114;

    // TODO: Consider requiring an explicit DiscardAlpha parameter

    public int DestXOffset { get; init; }

    public int DestYOffset { get; init; }

    public float BlackWhiteThreshold { get; init; }

    protected override LockMode SrcLockMode => LockMode.ReadOnly;

    protected override LockMode DstLockMode => LockMode.WriteOnly;

    protected override void ValidateCore(BitwiseImageData src, BitwiseImageData dst)
    {
        if (dst.w - DestXOffset < src.w || dst.h - DestYOffset < src.h)
        {
            throw new ArgumentException();
        }
        if (src.invertY)
        {
            throw new ArgumentException();
        }
    }

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        if (src.BitLayout == dst.BitLayout)
        {
            FastCopy(src, dst, partStart, partEnd);
        }
        else if (src.bytesPerPixel is 1 or 3 or 4 && dst.bytesPerPixel is 1 or 3 or 4)
        {
            RgbaCopy(src, dst, partStart, partEnd);
        }
        else if (src.bitsPerPixel == 1 && dst.bytesPerPixel is 1 or 3 or 4)
        {
            BitToRgbCopy(src, dst, partStart, partEnd);
        }
        else if (dst.bitsPerPixel == 1 && src.bytesPerPixel is 1 or 3 or 4)
        {
            RgbToBitCopy(src, dst, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported copy parameters");
        }
    }

    private unsafe void RgbaCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        bool copyAlpha = src.bytesPerPixel == 4 && dst.bytesPerPixel == 4;
        bool copyFromGray = src.bytesPerPixel == 1;
        bool copyToGray = dst.bytesPerPixel == 1;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + (j + DestXOffset) * dst.bytesPerPixel;
                byte r, g, b;
                if (copyFromGray)
                {
                    r = g = b = *srcPixel;
                }
                else
                {
                    r = *(srcPixel + src.rOff);
                    g = *(srcPixel + src.gOff);
                    b = *(srcPixel + src.bOff);
                }
                if (copyToGray)
                {
                    var luma = (byte) ((r * R_MULT + g * G_MULT + b * B_MULT) / 1000);
                    *dstPixel = luma;
                }
                else
                {
                    *(dstPixel + dst.rOff) = r;
                    *(dstPixel + dst.gOff) = g;
                    *(dstPixel + dst.bOff) = b;
                }
                if (copyAlpha)
                {
                    *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
                }
            }
        }
    }

    private unsafe void RgbToBitCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        bool copyFromGray = src.bytesPerPixel == 1;
        var thresholdAdjusted = ((int) (BlackWhiteThreshold * 1000) + 1000) * 255 / 2;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < src.w; j += 8)
            {
                byte monoByte = 0;
                for (int k = 0; k < 8; k++)
                {
                    monoByte <<= 1;
                    if (j + k < src.w)
                    {
                        var srcPixel = srcRow + (j + k) * src.bytesPerPixel;
                        int luma;
                        if (copyFromGray)
                        {
                            luma = *(srcPixel + src.rOff) * 1000;
                        }
                        else
                        {
                            var r = *(srcPixel + src.rOff);
                            var g = *(srcPixel + src.gOff);
                            var b = *(srcPixel + src.bOff);
                            luma = r * R_MULT + g * G_MULT + b * B_MULT;
                        }
                        if (luma >= thresholdAdjusted)
                        {
                            monoByte |= 1;
                        }
                    }
                }
                *(dstRow + j / 8) = monoByte;
            }
        }
    }

    private unsafe void BitToRgbCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        bool copyToGray = dst.bytesPerPixel == 1;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < src.w; j += 8)
            {
                byte monoByte = *(srcRow + j / 8);
                for (int k = 7; k >= 0; k--)
                {
                    var bit = monoByte & 1;
                    monoByte >>= 1;
                    if (j + k < src.w)
                    {
                        var dstPixel = dstRow + (j + k) * dst.bytesPerPixel;
                        var luma = (byte) (bit == 0 ? 0 : 255);
                        if (copyToGray)
                        {
                            *dstPixel = luma;
                        }
                        else
                        {
                            *(dstPixel + dst.rOff) = luma;
                            *(dstPixel + dst.gOff) = luma;
                            *(dstPixel + dst.bOff) = luma;
                        }
                    }
                }
            }
        }
    }

    private unsafe void FastCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        var bytesPerRow = src.bytesPerPixel * src.w;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstRow = dst.ptr + dst.stride * (i + DestYOffset) + DestXOffset * dst.bytesPerPixel;
            Buffer.MemoryCopy(srcRow, dstRow, bytesPerRow, bytesPerRow);
        }
    }
}