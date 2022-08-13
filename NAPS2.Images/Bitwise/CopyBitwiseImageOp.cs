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
        else if (src.bytesPerPixel == 1 && dst.bytesPerPixel is 3 or 4)
        {
            GrayToRgbCopy(src, dst);
        }
        else if (src.bitsPerPixel == 1 && dst.bytesPerPixel is 3 or 4)
        {
            BitToRgbCopy(src, dst);
        }
        else if (dst.bitsPerPixel == 1 && src.bytesPerPixel is 3 or 4)
        {
            RgbToBitCopy(src, dst);
        }
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
                    var luma = (byte) ((r * R_MULT + g * G_MULT + b * B_MULT) / 1000);
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
                var luma = (byte) ((r * R_MULT + g * G_MULT + b * B_MULT) / 1000);
                *dstPixel = luma;
            }
        }
    }

    private unsafe void GrayToRgbCopy(PixelInfo src, PixelInfo dst)
    {
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset);
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j;
                var dstPixel = dstRow + j * dst.bytesPerPixel + DestXOffset;
                var luma = *srcPixel;
                *(dstPixel + dst.rOff) = luma;
                *(dstPixel + dst.gOff) = luma;
                *(dstPixel + dst.bOff) = luma;
            }
        }
    }

    private unsafe void RgbToBitCopy(PixelInfo src, PixelInfo dst)
    {
        var thresholdAdjusted = ((int) (BlackWhiteThreshold * 1000) + 1000) * 255 / 2;
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset);
            for (int j = 0; j < src.w; j += 8)
            {
                byte monoByte = 0;
                for (int k = 0; k < 8; k++)
                {
                    monoByte <<= 1;
                    if (j + k < src.w)
                    {
                        var srcPixel = srcRow + (j + k) * src.bytesPerPixel;
                        byte r = *(srcPixel + src.rOff);
                        byte g = *(srcPixel + src.gOff);
                        byte b = *(srcPixel + src.bOff);
                        int luma = r * R_MULT + g * G_MULT + b * B_MULT;
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

    private unsafe void BitToRgbCopy(PixelInfo src, PixelInfo dst)
    {
        for (int i = 0; i < src.h; i++)
        {
            var srcRow = src.data + src.stride * i;
            var dstRow = dst.data + dst.stride * (i + DestYOffset);
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
                        *(dstPixel + dst.rOff) = luma;
                        *(dstPixel + dst.gOff) = luma;
                        *(dstPixel + dst.bOff) = luma;
                    }
                }
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