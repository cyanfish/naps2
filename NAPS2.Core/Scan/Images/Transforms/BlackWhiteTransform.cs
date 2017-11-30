using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class BlackWhiteTransform : Transform
    {
        public int Threshold { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            int bytesPerPixel;
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                bytesPerPixel = 3;
            }
            else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                bytesPerPixel = 4;
            }
            else
            {
                return bitmap;
            }

            int thresholdAdjusted = (Threshold + 1000) * 255 / 2;

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = Math.Abs(data.Stride);
            var bytes = new byte[stride * data.Height];
            var bits = new byte[data.Width * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    int r = bytes[stride * y + x * bytesPerPixel];
                    int g = bytes[stride * y + x * bytesPerPixel + 1];
                    int b = bytes[stride * y + x * bytesPerPixel + 2];
                    // Use standard values for grayscale conversion to weight the RGB values
                    int luma = r * 299 + g * 587 + b * 114;
                    if (luma >= thresholdAdjusted)
                    {
                        bits[data.Width * y + x] = 1;
                    }
                }
            }

            var monoBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format1bppIndexed);
            var p = monoBitmap.Palette;
            p.Entries[0] = Color.Black;
            p.Entries[1] = Color.White;
            monoBitmap.Palette = p;

            data = monoBitmap.LockBits(new Rectangle(0, 0, monoBitmap.Width, monoBitmap.Height), ImageLockMode.WriteOnly, monoBitmap.PixelFormat);
            stride = Math.Abs(data.Stride);
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x += 8)
                {
                    byte b = 0;
                    for (int k = 0; k < 8; k++)
                    {
                        b <<= 1;
                        if (x + k < data.Width)
                        {
                            b |= bits[y * data.Width + x + k];
                        }
                    }
                    Marshal.WriteByte(data.Scan0 + y * stride + x / 8, b);
                }
            }
            monoBitmap.UnlockBits(data);
            monoBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            bitmap.Dispose();

            return monoBitmap;
        }
    }
}
