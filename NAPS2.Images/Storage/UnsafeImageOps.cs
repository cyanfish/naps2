using System.Collections;

namespace NAPS2.Images.Storage;

// TODO: Make internal?
// TODO: Improve preconditions to guard against misuse causing access violations
// TODO: Verify test coverage
// TODO: If we add a new IMemoryImage implementation, we need to figure out RGB vs BGR (bgr is the Bitmap default)
// TODO: A lot of the r/g/b naming is off, should be b/g/r (I only corrected it where colors are treated differently)
public static class UnsafeImageOps
{
    public static unsafe void HueShift(IMemoryImage bitmap, float hueShift)
    {
        int bytesPerPixel = GetBytesPerPixel(bitmap);

        hueShift /= 60;

        using var lockState = bitmap.Lock(LockMode.ReadWrite, out var scan0, out var stride);
        int h = bitmap.Height;
        int w = bitmap.Width;

        byte* data = (byte*)scan0;
        PartitionRows(h, (start, end) =>
        {
            for (int y = start; y < end; y++)
            {
                byte* row = data + stride * y;
                for (int x = 0; x < w; x++)
                {
                    byte* pixel = row + x * bytesPerPixel;
                    byte r = *pixel;
                    byte g = *(pixel + 1);
                    byte b = *(pixel + 2);

                    int max = Math.Max(r, Math.Max(g, b));
                    int min = Math.Min(r, Math.Min(g, b));

                    if (max == min)
                    {
                        continue;
                    }

                    float hue = 0.0f;
                    float delta = max - min;
                    if (r == max)
                    {
                        hue = (g - b) / delta;
                    }
                    else if (g == max)
                    {
                        hue = 2 + (b - r) / delta;
                    }
                    else if (b == max)
                    {
                        hue = 4 + (r - g) / delta;
                    }
                    hue += hueShift;
                    hue = (hue + 6) % 6;

                    float sat = (max == 0) ? 0 : 1f - (1f * min / max);
                    float val = max;

                    int hi = (int)Math.Floor(hue);
                    float f = hue - hi;

                    byte v = (byte)(val);
                    byte p = (byte)(val * (1 - sat));
                    byte q = (byte)(val * (1 - f * sat));
                    byte t = (byte)(val * (1 - (1 - f) * sat));

                    if (hi == 0)
                    {
                        r = v;
                        g = t;
                        b = p;
                    }
                    else if (hi == 1)
                    {
                        r = q;
                        g = v;
                        b = p;
                    }
                    else if (hi == 2)
                    {
                        r = p;
                        g = v;
                        b = t;
                    }
                    else if (hi == 3)
                    {
                        r = p;
                        g = q;
                        b = v;
                    }
                    else if (hi == 4)
                    {
                        r = t;
                        g = p;
                        b = v;
                    }
                    else
                    {
                        r = v;
                        g = p;
                        b = q;
                    }

                    *pixel = r;
                    *(pixel + 1) = g;
                    *(pixel + 2) = b;
                }
            }
        });
    }

    public static unsafe void RowWiseCopy(IMemoryImage src, IMemoryImage dst, int x1, int y1, int w, int h)
    {
        bool bitPerPixel = src.PixelFormat == ImagePixelFormat.BW1;
        int bytesPerPixel = bitPerPixel ? 0 : GetBytesPerPixel(src);

        using var srcLockState = src.Lock(LockMode.ReadWrite, out var srcScan0, out var srcStride);
        using var dstLockState = dst.Lock(LockMode.ReadWrite, out var dstScan0, out var dstStride);

        if (bitPerPixel)
        {
            // 1bpp copy requires bit shifting
            int shift1 = x1 % 8;
            int shift2 = 8 - shift1;

            int bytes = (w + 7) / 8;
            int bytesExceptLast = bytes - 1;

            PartitionRows(h, (start, end) =>
            {
                for (int y = start; y < end; y++)
                {
                    byte* srcRow = (byte*)(srcScan0 + srcStride * (y1 + y) + (x1 + 7) / 8);
                    byte* dstRow = (byte*)(dstScan0 + dstStride * y);

                    for (int x = 0; x < bytesExceptLast; x++)
                    {
                        byte* srcPtr = srcRow + x;
                        byte* dstPtr = dstRow + x;
                        *dstPtr = (byte)((*srcPtr << shift1) | (*(srcPtr + 1) >> shift2));
                    }
                    if (w > 0)
                    {
                        byte* srcPtr = srcRow + bytesExceptLast;
                        byte* dstPtr = dstRow + bytesExceptLast;
                        if (shift2 == 8)
                        {
                            *dstPtr = *srcPtr;
                        }
                        else
                        {
                            *dstPtr = (byte)((*srcPtr << shift1) | (*(srcPtr + 1) >> shift2));
                        }
                        byte mask = (byte)(0xFF << (w % 8));
                        *dstPtr = (byte)(*dstPtr & mask);
                    }
                }
            });
        }
        else
        {
            // Byte-aligned copy is a bit simpler
            // PartitionRows(h, (start, end) =>
            // {
                for (int y = 0; y < h; y++)
                {
                    byte* srcPtrB = (byte*)(srcScan0 + srcStride * (y1 + y) + x1 * bytesPerPixel);
                    byte* dstPtrB = (byte*)(dstScan0 + dstStride * y);
                    long* srcPtrL = (long*)srcPtrB;
                    long* dstPtrL = (long*)dstPtrB;
                    var len = w * bytesPerPixel;

                    // Copy via longs for better bandwidth
                    for (int i = 0; i < len / 8; i++)
                    {
                        *(dstPtrL + i) = *(srcPtrL + i);
                    }
                    // Then copy any leftovers one byte at a time
                    for (int i = len / 8 * 8; i < len; i++)
                    {
                        *(dstPtrB + i) = *(srcPtrB + i);
                    }
                }
            // });
        }
    }

    private static int GetBytesPerPixel(IMemoryImage bitmap)
    {
        if (bitmap.PixelFormat == ImagePixelFormat.ARGB32)
        {
            return 4;
        }
        else if (bitmap.PixelFormat == ImagePixelFormat.RGB24)
        {
            return 3;
        }
        else
        {
            throw new ArgumentException("Unsupported pixel format: " + bitmap.PixelFormat);
        }
    }

    private static void PartitionRows(int count, Action<int, int> action)
    {
        const int partitionCount = 8;
        int div = (count + partitionCount - 1) / partitionCount;

        var tasks = new Task[partitionCount];
        for (int i = 0; i < partitionCount; i++)
        {
            int start = div * i, end = Math.Min(div * (i + 1), count);
            tasks[i] = Task.Run(() => action(start, end));
        }
        Task.WaitAll(tasks);
    }
}