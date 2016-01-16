using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images
{
    public class ThresholdBlankDetector : IBlankDetector
    {
        // If the pixel value <= THRESHOLD (0-254), then it counts as a non-white pixel.
        private const int THRESHOLD = 180;
        // If the fraction of non-white pixels > (1.0 - sensitivity) * SENSITIVITY_SCALE, then it counts as a non-blank page.
        private const double SENSITIVITY_SCALE = 0.005;

        public bool IsBlank(Bitmap bitmap, double sensitivity)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                using (var bitmap2 = BitmapHelper.CopyToBpp(bitmap, 8))
                {
                    return IsBlankRGB(bitmap2, sensitivity);
                }
            }
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                return false;
            }
            return IsBlankRGB(bitmap, sensitivity);
        }

        private static bool IsBlankRGB(Bitmap bitmap, double sensitivity)
        {
            long totalPixels = bitmap.Width * bitmap.Height;
            long matchPixels = 0;

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var stride = Math.Abs(data.Stride);
            var bytes = new byte[stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);
            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    int r = bytes[stride * y + x * 3];
                    int g = bytes[stride * y + x * 3 + 1];
                    int b = bytes[stride * y + x * 3 + 2];
                    if (r + g + b <= THRESHOLD * 3)
                    {
                        matchPixels++;
                    }
                }
            }

            return (matchPixels / (double)totalPixels) < (1.0 - sensitivity) * SENSITIVITY_SCALE;
        }
    }
}