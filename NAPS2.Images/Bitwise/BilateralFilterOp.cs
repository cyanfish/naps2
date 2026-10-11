#if NET8_0_OR_GREATER
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
#endif

namespace NAPS2.Images.Bitwise;

/// <summary>
/// Runs a bilateral filter operation, which reduces noise without losing edges or fine details.
/// https://en.wikipedia.org/wiki/Bilateral_filter
/// </summary>
internal class BilateralFilterOp : BinaryBitwiseImageOp
{
    // The color distance (in the 0-255 range) at which pixels are weighted to 0.
    // The weight linearly scales up as the color distance approaches 0.
    private const int COLOR_DIST_MAX = 32;

    // The size of the square around the current pixel to check.
    // Should be an odd number for symmetry.
    // Pixels in the square are weighted by how close they are to the center.
    private const int FILTER_SIZE = 15;

    protected override LockMode SrcLockMode => LockMode.ReadOnly;
    protected override LockMode DstLockMode => LockMode.WriteOnly;

    internal bool AllowVectorization { get; init; } = true;

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        if (src.bytesPerPixel is 3 or 4 && dst.bytesPerPixel is 3 or 4)
        {
#if NET8_0_OR_GREATER
            if (AllowVectorization && CanVectorize(src))
            {
                PerformRgbaVectorized(src, dst, partStart, partEnd);
                return;
            }
#endif
            PerformRgba(src, dst, partStart, partEnd);
        }
        else if (src.bytesPerPixel == 1 && dst.bytesPerPixel == 1)
        {
#if NET8_0_OR_GREATER
            if (AllowVectorization && CanVectorize(src))
            {
                PerformGrayVectorized(src, dst, partStart, partEnd);
                return;
            }
#endif
            PerformGray(src, dst, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        bool copyAlpha = src.hasAlpha && dst.hasAlpha;
        const int s = FILTER_SIZE / 2;

        var filter = BuildFilter();
        var diffWeights = BuildRgbaDiffWeights();

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
                    bool skipPixel = r == 255 && g == 255 && b == 255;
                    if (!skipPixel)
                    {
                        int rTotal = 0, gTotal = 0, bTotal = 0;
                        int weightTotal = 0;
                        for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                        {
                            for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
                            {
                                int imageX = j - s + filterX;
                                int imageY = i - s + filterY;

                                var pixel = src.ptr + src.stride * imageY + src.bytesPerPixel * imageX;

                                var r2 = *(pixel + src.rOff);
                                var g2 = *(pixel + src.gOff);
                                var b2 = *(pixel + src.bOff);

                                // TODO: Better color distance
                                var diff = (r + g + b) - (r2 + g2 + b2) + 256 * 3;
                                var weight = filter[filterX + filterY * FILTER_SIZE] * diffWeights[diff];
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

    private unsafe void PerformGray(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        const int s = FILTER_SIZE / 2;

        var filter = BuildFilter();
        var diffWeights = BuildGrayDiffWeights();

        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstRow = dst.ptr + dst.stride * i;
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + j * dst.bytesPerPixel;
                int lum = *srcPixel;

                if (j > s && j < src.w - s && i > s && i < src.h - s)
                {
                    bool skipPixel = false;
                    if (lum == 255)
                    {
                        var prevPixel = src.ptr + src.stride * i + src.bytesPerPixel * j - 1;
                        var nextPixel = src.ptr + src.stride * i + src.bytesPerPixel * j + 1;
                        byte prev = *prevPixel;
                        byte next = *nextPixel;
                        if (prev == 255 && next == 255)
                        {
                            skipPixel = true;
                        }
                    }
                    if (!skipPixel)
                    {
                        int lumTotal = 0;
                        int weightTotal = 0;
                        for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
                        {
                            for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                            {
                                int imageX = j - s + filterX;
                                int imageY = i - s + filterY;

                                var pixel = src.ptr + src.stride * imageY + src.bytesPerPixel * imageX;

                                var lum2 = *pixel;

                                var diff = lum - lum2 + 256;
                                var weight = filter[filterX + filterY * FILTER_SIZE] * diffWeights[diff];
                                weightTotal += weight;
                                lumTotal += lum2 * weight;
                            }
                        }
                        lum = lumTotal / weightTotal;
                    }
                }
                *dstPixel = (byte) lum;
            }
        }
    }

    private static int[] BuildFilter()
    {
        const int s = FILTER_SIZE / 2;
        var filter = new int[FILTER_SIZE * FILTER_SIZE];
        for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
        {
            for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
            {
                int dx = filterX - s;
                int dy = filterY - s;
                var dmax = Math.Sqrt(2 * s * s);
                var d = Math.Sqrt(dx * dx + dy * dy) / dmax;
                filter[filterX + filterY * FILTER_SIZE] = (int) ((1 - d) * 256);
            }
        }
        return filter;
    }

    private static int[] BuildRgbaDiffWeights()
    {
        var diffWeights = new int[256 * 3 * 2];
        for (int i = 0; i < COLOR_DIST_MAX * 3; i++)
        {
            diffWeights[256 * 3 + i] = COLOR_DIST_MAX - i / 3;
            diffWeights[256 * 3 - i] = COLOR_DIST_MAX - i / 3;
        }
        return diffWeights;
    }

    private static int[] BuildGrayDiffWeights()
    {
        var diffWeights = new int[256 * 2];
        for (int i = 0; i < COLOR_DIST_MAX; i++)
        {
            diffWeights[256 + i] = COLOR_DIST_MAX - i;
            diffWeights[256 - i] = COLOR_DIST_MAX - i;
        }
        return diffWeights;
    }

    // The vectorization implementation is fully AI-generated. Rather than a detailed review, we instead verify its
    // accuracy by a set of unit tests that ensure pixel-for-pixel accuracy with the non-vectorized implementation.
    #region Vectorization

#if NET8_0_OR_GREATER
    // The vectorized implementations give identical results to PerformRgba/PerformGray, but compute several adjacent
    // output pixels at once. To make that efficient, source rows are first converted to planar int buffers. These hold
    // the FILTER_SIZE rows needed for the current output row and are reused as a ring buffer (row y is at slot
    // y % FILTER_SIZE).

    private static bool CanVectorize(BitwiseImageData src)
    {
        const int s = FILTER_SIZE / 2;
        // The filtered pixels in each row are s < j < w - s, which needs to fit at least one vector
        return Vector.IsHardwareAccelerated && src.w - 2 * s - 1 >= Vector<int>.Count;
    }

    private unsafe void PerformRgbaVectorized(BitwiseImageData src, BitwiseImageData dst, int partStart,
        int partEnd)
    {
        bool copyAlpha = src.hasAlpha && dst.hasAlpha;
        const int s = FILTER_SIZE / 2;
        int n = Vector<int>.Count;
        int w = src.w;

        var filter = BuildFilter();
        // The buffers are large enough to be on the LOH, so we use a pool to avoid repeated allocations
        var rBuf = ArrayPool<int>.Shared.Rent(FILTER_SIZE * w);
        var gBuf = ArrayPool<int>.Shared.Rent(FILTER_SIZE * w);
        var bBuf = ArrayPool<int>.Shared.Rent(FILTER_SIZE * w);
        var sumBuf = ArrayPool<int>.Shared.Rent(FILTER_SIZE * w);
        var rowOffsets = new int[FILTER_SIZE];
        int lastLoadedRow = -1;

        var zero = Vector<int>.Zero;
        var maxColorWeight = new Vector<int>(COLOR_DIST_MAX);
        var white = new Vector<int>(255 * 3);
        // (x * 21846) >> 16 == x / 3 for 0 <= x <= 765
        var div3Multiplier = new Vector<int>(21846);

        try
        {
            fixed (int* rPtr = rBuf, gPtr = gBuf, bPtr = bBuf, sumPtr = sumBuf)
            {
                for (int i = partStart; i < partEnd; i++)
                {
                    var srcRow = src.ptr + src.stride * i;
                    var dstRow = dst.ptr + dst.stride * i;
                    if (i <= s || i >= src.h - s)
                    {
                        CopyRgbaPixels(src, dst, srcRow, dstRow, 0, w, copyAlpha);
                        continue;
                    }

                    for (int y = Math.Max(lastLoadedRow + 1, i - s); y <= i + s; y++)
                    {
                        int slot = y % FILTER_SIZE * w;
                        var row = src.ptr + src.stride * y;
                        for (int x = 0; x < w; x++)
                        {
                            var pixel = row + x * src.bytesPerPixel;
                            int r = *(pixel + src.rOff);
                            int g = *(pixel + src.gOff);
                            int b = *(pixel + src.bOff);
                            rPtr[slot + x] = r;
                            gPtr[slot + x] = g;
                            bPtr[slot + x] = b;
                            sumPtr[slot + x] = r + g + b;
                        }
                    }
                    lastLoadedRow = i + s;
                    for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                    {
                        rowOffsets[filterY] = (i - s + filterY) % FILTER_SIZE * w;
                    }

                    CopyRgbaPixels(src, dst, srcRow, dstRow, 0, s + 1, copyAlpha);
                    CopyRgbaPixels(src, dst, srcRow, dstRow, w - s, w, copyAlpha);

                    for (int blockStart = s + 1; blockStart < w - s; blockStart += n)
                    {
                        // Shift the last block left so it stays in bounds (recomputing some pixels)
                        int j0 = Math.Min(blockStart, w - s - n);

                        var center = Unsafe.ReadUnaligned<Vector<int>>(sumPtr + rowOffsets[s] + j0);
                        if (Vector.EqualsAll(center, white))
                        {
                            // Pure white pixels are left as-is
                            CopyRgbaPixels(src, dst, srcRow, dstRow, j0, j0 + n, copyAlpha);
                            continue;
                        }

                        var weightTotal = zero;
                        var rTotal = zero;
                        var gTotal = zero;
                        var bTotal = zero;
                        for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                        {
                            int rowOffset = rowOffsets[filterY] + j0 - s;
                            for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
                            {
                                int offset = rowOffset + filterX;
                                var diff = Vector.Abs(center - Unsafe.ReadUnaligned<Vector<int>>(sumPtr + offset));
                                // Equivalent to the BuildRgbaDiffWeights lookup
                                var diffWeight = Vector.Max(zero,
                                    maxColorWeight - Vector.ShiftRightLogical(diff * div3Multiplier, 16));
                                var weight = diffWeight * filter[filterX + filterY * FILTER_SIZE];
                                weightTotal += weight;
                                rTotal += Unsafe.ReadUnaligned<Vector<int>>(rPtr + offset) * weight;
                                gTotal += Unsafe.ReadUnaligned<Vector<int>>(gPtr + offset) * weight;
                                bTotal += Unsafe.ReadUnaligned<Vector<int>>(bPtr + offset) * weight;
                            }
                        }

                        for (int k = 0; k < n; k++)
                        {
                            if (center[k] == 255 * 3)
                            {
                                CopyRgbaPixels(src, dst, srcRow, dstRow, j0 + k, j0 + k + 1, copyAlpha);
                                continue;
                            }
                            var srcPixel = srcRow + (j0 + k) * src.bytesPerPixel;
                            var dstPixel = dstRow + (j0 + k) * dst.bytesPerPixel;
                            *(dstPixel + dst.rOff) = (byte) (rTotal[k] / weightTotal[k]);
                            *(dstPixel + dst.gOff) = (byte) (gTotal[k] / weightTotal[k]);
                            *(dstPixel + dst.bOff) = (byte) (bTotal[k] / weightTotal[k]);
                            if (copyAlpha)
                            {
                                *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rBuf);
            ArrayPool<int>.Shared.Return(gBuf);
            ArrayPool<int>.Shared.Return(bBuf);
            ArrayPool<int>.Shared.Return(sumBuf);
        }
    }

    private unsafe void PerformGrayVectorized(BitwiseImageData src, BitwiseImageData dst, int partStart,
        int partEnd)
    {
        const int s = FILTER_SIZE / 2;
        int n = Vector<int>.Count;
        int w = src.w;

        var filter = BuildFilter();
        // The buffer is large enough to be on the LOH, so we use a pool to avoid repeated allocations
        var lumBuf = ArrayPool<int>.Shared.Rent(FILTER_SIZE * w);
        var rowOffsets = new int[FILTER_SIZE];
        int lastLoadedRow = -1;

        var zero = Vector<int>.Zero;
        var maxColorWeight = new Vector<int>(COLOR_DIST_MAX);
        var white = new Vector<int>(255);

        try
        {
            fixed (int* lumPtr = lumBuf)
            {
                for (int i = partStart; i < partEnd; i++)
                {
                    var srcRow = src.ptr + src.stride * i;
                    var dstRow = dst.ptr + dst.stride * i;
                    if (i <= s || i >= src.h - s)
                    {
                        new Span<byte>(srcRow, w).CopyTo(new Span<byte>(dstRow, w));
                        continue;
                    }

                    for (int y = Math.Max(lastLoadedRow + 1, i - s); y <= i + s; y++)
                    {
                        int slot = y % FILTER_SIZE * w;
                        var row = src.ptr + src.stride * y;
                        for (int x = 0; x < w; x++)
                        {
                            lumPtr[slot + x] = row[x];
                        }
                    }
                    lastLoadedRow = i + s;
                    for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                    {
                        rowOffsets[filterY] = (i - s + filterY) % FILTER_SIZE * w;
                    }

                    new Span<byte>(srcRow, s + 1).CopyTo(new Span<byte>(dstRow, s + 1));
                    new Span<byte>(srcRow + w - s, s).CopyTo(new Span<byte>(dstRow + w - s, s));

                    var centerRow = lumPtr + rowOffsets[s];
                    for (int blockStart = s + 1; blockStart < w - s; blockStart += n)
                    {
                        // Shift the last block left so it stays in bounds (recomputing some pixels)
                        int j0 = Math.Min(blockStart, w - s - n);

                        var center = Unsafe.ReadUnaligned<Vector<int>>(centerRow + j0);
                        // Pure white pixels with white horizontal neighbors are left as-is. As values are at most
                        // 255, the bitwise AND is 255 only if all three are 255.
                        var skip = center &
                                   Unsafe.ReadUnaligned<Vector<int>>(centerRow + j0 - 1) &
                                   Unsafe.ReadUnaligned<Vector<int>>(centerRow + j0 + 1);
                        if (Vector.EqualsAll(skip, white))
                        {
                            new Span<byte>(srcRow + j0, n).CopyTo(new Span<byte>(dstRow + j0, n));
                            continue;
                        }

                        var weightTotal = zero;
                        var lumTotal = zero;
                        for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                        {
                            int rowOffset = rowOffsets[filterY] + j0 - s;
                            for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
                            {
                                var lum2 = Unsafe.ReadUnaligned<Vector<int>>(lumPtr + rowOffset + filterX);
                                // Equivalent to the BuildGrayDiffWeights lookup
                                var diffWeight = Vector.Max(zero, maxColorWeight - Vector.Abs(center - lum2));
                                var weight = diffWeight * filter[filterX + filterY * FILTER_SIZE];
                                weightTotal += weight;
                                lumTotal += lum2 * weight;
                            }
                        }

                        for (int k = 0; k < n; k++)
                        {
                            dstRow[j0 + k] = skip[k] == 255
                                ? srcRow[j0 + k]
                                : (byte) (lumTotal[k] / weightTotal[k]);
                        }
                    }
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(lumBuf);
        }
    }

    private static unsafe void CopyRgbaPixels(BitwiseImageData src, BitwiseImageData dst, byte* srcRow,
        byte* dstRow, int start, int end, bool copyAlpha)
    {
        for (int j = start; j < end; j++)
        {
            var srcPixel = srcRow + j * src.bytesPerPixel;
            var dstPixel = dstRow + j * dst.bytesPerPixel;
            *(dstPixel + dst.rOff) = *(srcPixel + src.rOff);
            *(dstPixel + dst.gOff) = *(srcPixel + src.gOff);
            *(dstPixel + dst.bOff) = *(srcPixel + src.bOff);
            if (copyAlpha)
            {
                *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
            }
        }
    }
#endif

    #endregion
}