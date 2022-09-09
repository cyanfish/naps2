namespace NAPS2.Images.Bitwise;

// TODO: Need to double check callers set resolution when needed
public class CopyBitwiseImageOp : BinaryBitwiseImageOp
{
    private const int R_MULT = 299;
    private const int G_MULT = 587;
    private const int B_MULT = 114;

    // TODO: Consider requiring an explicit DiscardAlpha parameter

    public int SourceXOffset { get; set; }
    public int SourceYOffset { get; set; }
    public int DestXOffset { get; init; }
    public int DestYOffset { get; init; }
    public ColorChannel DestChannel { get; init; }
    public int? Columns { get; init; }
    public int? Rows { get; init; }

    protected override int GetPartitionSize(BitwiseImageData src, BitwiseImageData dst)
    {
        return Rows ?? src.h;
    }

    public float BlackWhiteThreshold { get; init; }

    protected override LockMode SrcLockMode => LockMode.ReadOnly;

    protected override LockMode DstLockMode => LockMode.WriteOnly;

    protected override void ValidateCore(BitwiseImageData src, BitwiseImageData dst)
    {
        var w = Columns ?? src.w;
        var h = Rows ?? src.h;
        if (SourceXOffset < 0 || SourceYOffset < 0 || DestXOffset < 0 || DestYOffset < 0)
        {
            throw new ArgumentException(
                $"X/Y offsets must be non-negative: {SourceXOffset} {SourceYOffset} {DestXOffset} {DestYOffset}");
        }
        if (SourceXOffset + w > src.w || SourceYOffset + h > src.h ||
            dst.w - DestXOffset < w || dst.h - DestYOffset < h)
        {
            throw new ArgumentException("X/Y offsets must be within the row/column counts. " +
                                        $"Offsets: {SourceXOffset} {SourceYOffset} {DestXOffset} {DestYOffset}; " +
                                        $"Copy dimensions: {w} {h}; " +
                                        $"Source dimensions: {src.w} {src.h}; " +
                                        $"Destination dimensions: {dst.w} {dst.h}");
        }
        if (src.invertY)
        {
            throw new ArgumentException("Source Y inversion not supported");
        }
        if (DestChannel != ColorChannel.All &&
            (src.bytesPerPixel is not (1 or 3 or 4) || dst.bytesPerPixel is not (3 or 4)))
        {
            throw new ArgumentException(
                "DestChannel is only supported when the source is grayscale/color and the destination is color.");
        }
        if ((src.invertColorSpace || dst.invertColorSpace) && (src.bitsPerPixel != 1 || dst.bitsPerPixel != 1))
        {
            throw new ArgumentException(
                "SubPixelType.InvertedBit is only supported when both source and destination are 1 bit per pixel");
        }
    }

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        if (src.BitLayout == dst.BitLayout &&
            (src.bytesPerPixel > 0 || (SourceXOffset % 8 == 0 && DestXOffset % 8 == 0)) &&
            DestChannel == ColorChannel.All)
        {
            FastCopy(src, dst, partStart, partEnd);
        }
        else if (src.bytesPerPixel is 1 or 3 or 4 && dst.bytesPerPixel is 1 or 3 or 4)
        {
            RgbaCopy(src, dst, partStart, partEnd);
        }
        else if (src.bitsPerPixel == 1 && dst.bytesPerPixel is 1 or 3 or 4 && SourceXOffset % 8 == 0)
        {
            BitToRgbCopy(src, dst, partStart, partEnd);
        }
        else if (dst.bitsPerPixel == 1 && src.bytesPerPixel is 1 or 3 or 4 && DestXOffset % 8 == 0)
        {
            RgbToBitCopy(src, dst, partStart, partEnd);
        }
        else if (src.bitsPerPixel == 1 && dst.bitsPerPixel == 1)
        {
            UnalignedBitCopy(src, dst, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported copy parameters");
        }
        if (src.invertColorSpace ^ dst.invertColorSpace)
        {
            Invert(dst, partStart, partEnd);
        }
    }

    private unsafe void RgbaCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        var w = Columns ?? src.w;
        bool copyAlpha = src.bytesPerPixel == 4 && dst.bytesPerPixel == 4;
        bool copyFromGray = src.bytesPerPixel == 1;
        bool copyToRed = dst.bytesPerPixel != 1 && DestChannel is ColorChannel.All or ColorChannel.Red;
        bool copyToGreen = dst.bytesPerPixel != 1 && DestChannel is ColorChannel.All or ColorChannel.Green;
        bool copyToBlue = dst.bytesPerPixel != 1 && DestChannel is ColorChannel.All or ColorChannel.Blue;
        bool copyToGray = dst.bytesPerPixel == 1;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * (i + SourceYOffset);
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < w; j++)
            {
                var srcPixel = srcRow + (j + SourceXOffset) * src.bytesPerPixel;
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
                if (copyToRed) *(dstPixel + dst.rOff) = r;
                if (copyToGreen) *(dstPixel + dst.gOff) = g;
                if (copyToBlue) *(dstPixel + dst.bOff) = b;
                if (copyAlpha) *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
            }
        }
    }

    private unsafe void RgbToBitCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        var w = Columns ?? src.w;
        bool copyFromGray = src.bytesPerPixel == 1;
        var thresholdAdjusted = ((int) (BlackWhiteThreshold * 1000) + 1000) * 255 / 2;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * (i + SourceYOffset);
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < w; j += 8)
            {
                byte dstByte = 0;
                for (int k = 0; k < 8; k++)
                {
                    dstByte <<= 1;
                    if (j + k < src.w)
                    {
                        var srcPixel = srcRow + (j + SourceXOffset + k) * src.bytesPerPixel;
                        int luma;
                        if (copyFromGray)
                        {
                            luma = *srcPixel * 1000;
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
                            dstByte |= 1;
                        }
                    }
                }
                *(dstRow + (j + DestXOffset) / 8) = dstByte;
            }
        }
    }

    private unsafe void BitToRgbCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        var w = Columns ?? src.w;
        bool copyToGray = dst.bytesPerPixel == 1;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * (i + SourceYOffset);
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < w; j += 8)
            {
                byte srcByte = *(srcRow + (j + SourceXOffset) / 8);
                for (int k = 7; k >= 0; k--)
                {
                    var bit = srcByte & 1;
                    srcByte >>= 1;
                    if (j + k < src.w)
                    {
                        var dstPixel = dstRow + (j + DestXOffset + k) * dst.bytesPerPixel;
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

    private unsafe void UnalignedBitCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        // TODO: This could be a lot faster if we use 64-bit (aligned) shifts & masks for the "middle" bytes
        var w = Columns ?? src.w;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * (i + SourceYOffset);
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY;
            for (int j = 0; j < w; j++)
            {
                var srcPixelIndex = j + SourceXOffset;
                var srcByte = *(srcRow + srcPixelIndex / 8);
                var bit = (srcByte >> (7 - srcPixelIndex % 8)) & 1;
                var dstPixelIndex = j + DestXOffset;
                var dstPtr = dstRow + dstPixelIndex / 8;
                var dstByte = *dstPtr;
                dstByte |= (byte) (bit << (7 - dstPixelIndex % 8));
                *dstPtr = dstByte;
            }
        }
    }

    private unsafe void FastCopy(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        var w = Columns ?? src.w;
        var bytesPerRow = (src.bitsPerPixel * w + 7) / 8;
        var srcXBytesOff = SourceXOffset * src.bitsPerPixel / 8;
        var dstXBytesOff = DestXOffset * dst.bitsPerPixel / 8;
        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * (i + SourceYOffset) + srcXBytesOff;
            var dstY = i + DestYOffset;
            if (dst.invertY)
            {
                dstY = dst.h - dstY - 1;
            }
            var dstRow = dst.ptr + dst.stride * dstY + dstXBytesOff;
            Buffer.MemoryCopy(srcRow, dstRow, bytesPerRow, bytesPerRow);
        }
    }

    private unsafe void Invert(BitwiseImageData data, int partStart, int partEnd)
    {
        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            // TODO: Optimize with long operations
            for (int j = 0; j < data.stride; j++)
            {
                var b = *(row + j);
                *(row + j) = (byte) (~b & 0xFF);
            }
        }
    }
}