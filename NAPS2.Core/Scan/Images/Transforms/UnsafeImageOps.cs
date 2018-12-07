using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NAPS2.Util;

namespace NAPS2.Scan.Images.Transforms
{
    public static class UnsafeImageOps
    {
        public static unsafe void ChangeBrightness(Bitmap bitmap, float brightnessAdjusted)
        {
            int bytesPerPixel = GetBytesPerPixel(bitmap);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int stride = Math.Abs(bitmapData.Stride);
            int h = bitmapData.Height;
            int w = bitmapData.Width;

            brightnessAdjusted *= 255;

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

                        int r2 = (int)(r + brightnessAdjusted);
                        int g2 = (int)(g + brightnessAdjusted);
                        int b2 = (int)(b + brightnessAdjusted);

                        r = (byte)(r2 < 0 ? 0 : r2 > 255 ? 255 : r2);
                        g = (byte)(g2 < 0 ? 0 : g2 > 255 ? 255 : g2);
                        b = (byte)(b2 < 0 ? 0 : b2 > 255 ? 255 : b2);

                        *pixel = r;
                        *(pixel + 1) = g;
                        *(pixel + 2) = b;
                    }
                }
            });

            bitmap.UnlockBits(bitmapData);
        }

        public static unsafe void ChangeContrast(Bitmap bitmap, float contrastAdjusted, float offset)
        {
            int bytesPerPixel = GetBytesPerPixel(bitmap);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int stride = Math.Abs(bitmapData.Stride);
            int h = bitmapData.Height;
            int w = bitmapData.Width;

            offset *= 255;

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

                        int r2 = (int)(r * contrastAdjusted + offset);
                        int g2 = (int)(g * contrastAdjusted + offset);
                        int b2 = (int)(b * contrastAdjusted + offset);

                        r = (byte)(r2 < 0 ? 0 : r2 > 255 ? 255 : r2);
                        g = (byte)(g2 < 0 ? 0 : g2 > 255 ? 255 : g2);
                        b = (byte)(b2 < 0 ? 0 : b2 > 255 ? 255 : b2);

                        *pixel = r;
                        *(pixel + 1) = g;
                        *(pixel + 2) = b;
                    }
                }
            });

            bitmap.UnlockBits(bitmapData);
        }

        public static unsafe void HueShift(Bitmap bitmap, float hueShift)
        {
            int bytesPerPixel = GetBytesPerPixel(bitmap);

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

        public static unsafe void RowWiseCopy(Bitmap src, Bitmap dst, int x1, int y1, int x2, int y2, int w, int h)
        {
            if (src.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                // 1bpp copy requires bit shifting, and we therefore must copy ourselves
                var srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, src.PixelFormat);
                int srcStride = Math.Abs(srcData.Stride);

                var dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.ReadWrite, dst.PixelFormat);
                int dstStride = Math.Abs(dstData.Stride);

                int shift1 = (x2 - x1) % 8;
                int shift2 = (x1 - x2) % 8;

                for (int y = 0; y < h; y++)
                {
                    byte* srcRow = (byte*)(srcData.Scan0 + srcStride * (y1 + y));
                    byte* dstRow = (byte*)(dstData.Scan0 + dstStride * (y2 + y));
                    for (int x = 0; x < w; x++)
                    {
                        byte* srcPixel = srcRow + (x + x1 + 7) / 8;
                        byte* dstPixel = dstRow + (x + x2 + 7) / 8;
                        *dstPixel = (byte)((*srcPixel << shift1) | (*(srcPixel + 1) << shift2));
                    }
                }

                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }
            else
            {
                // Byte-aligned copy can be optimized with a fast Win32 memcpy operation
                int bytesPerPixel = GetBytesPerPixel(src);

                var srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, src.PixelFormat);
                int srcStride = Math.Abs(srcData.Stride);

                var dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.ReadWrite, dst.PixelFormat);
                int dstStride = Math.Abs(dstData.Stride);

                for (int y = 0; y < h; y++)
                {
                    var srcPtr = srcData.Scan0 + srcStride * (y1 + y) + x1 * bytesPerPixel;
                    var dstPtr = dstData.Scan0 + dstStride * (y2 + y) + x2 * bytesPerPixel;
                    var len = (uint)(w * bytesPerPixel);
                    Win32.CopyMemory(dstPtr, srcPtr, len);
                }

                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }
        }

        private static int GetBytesPerPixel(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return 4;
            }
            else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return 3;
            }
            else
            {
                throw new ArgumentException("Unsupported pixel format: " + bitmap.PixelFormat);
            }
        }

        public static unsafe Bitmap ConvertTo1Bpp(Bitmap bitmap, int threshold)
        {
            int thresholdAdjusted = (threshold + 1000) * 255 / 2;
            int bytesPerPixel = GetBytesPerPixel(bitmap);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = Math.Abs(bitmapData.Stride);
            byte* data = (byte*)bitmapData.Scan0;
            int h = bitmapData.Height;
            int w = bitmapData.Width;

            var monoBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format1bppIndexed);
            var p = monoBitmap.Palette;
            p.Entries[0] = Color.Black;
            p.Entries[1] = Color.White;
            monoBitmap.Palette = p;
            var monoBitmapData = monoBitmap.LockBits(new Rectangle(0, 0, monoBitmap.Width, monoBitmap.Height), ImageLockMode.WriteOnly, monoBitmap.PixelFormat);
            var monoStride = Math.Abs(monoBitmapData.Stride);
            byte* monoData = (byte*)monoBitmapData.Scan0;

            PartitionRows(h, (start, end) =>
            {
                for (int y = start; y < end; y++)
                {
                    byte* row = data + stride * y;
                    for (int x = 0; x < w; x += 8)
                    {
                        byte monoByte = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            monoByte <<= 1;
                            if (x + k < w)
                            {
                                byte* pixel = row + (x + k) * bytesPerPixel;
                                byte r = *pixel;
                                byte g = *(pixel + 1);
                                byte b = *(pixel + 2);
                                // Use standard values for grayscale conversion to weight the RGB values
                                int luma = r * 299 + g * 587 + b * 114;
                                if (luma >= thresholdAdjusted)
                                {
                                    monoByte |= 1;
                                }
                            }
                        }
                        *(monoData + y * monoStride + x / 8) = monoByte;
                    }
                }
            });

            bitmap.UnlockBits(bitmapData);
            monoBitmap.UnlockBits(monoBitmapData);
            monoBitmap.SafeSetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            return monoBitmap;
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
