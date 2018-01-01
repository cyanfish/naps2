using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace NAPS2.Unsafe
{
    public static class UnsafeImageOps
    {
        public static unsafe void HueShift(Bitmap bitmap, int bytesPerPixel, float hueShift)
        {
            hueShift /= 60;

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int stride = Math.Abs(bitmapData.Stride);
            int h = bitmapData.Height;
            int w = bitmapData.Width;

            byte* data = (byte*)bitmapData.Scan0;
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

            bitmap.UnlockBits(bitmapData);
        }

        private static void PartitionRows(int count, Action<int, int> action)
        {
            const int partitionCount = 8;
            int div = (count + partitionCount - 1) / partitionCount;

            var tasks = new Task[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                int start = div * i, end = Math.Min(div * (i + 1), count);
                tasks[i] = Task.Factory.StartNew(() => action(start, end));
            }
            Task.WaitAll(tasks);
        }
    }
}
