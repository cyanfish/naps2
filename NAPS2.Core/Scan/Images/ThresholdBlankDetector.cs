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
        // If the pixel value (0-255) >= white_threshold, then it counts as a white pixel.
        private const int WHITE_THRESHOLD_MIN = 1;
        private const int WHITE_THRESHOLD_MAX = 255;
        // If the fraction of non-white pixels > coverage_threshold, then it counts as a non-blank page.
        private const double COVERAGE_THRESHOLD_MIN = 0.00;
        private const double COVERAGE_THRESHOLD_MAX = 0.01;

        public bool IsBlank(Bitmap bitmap, int whiteThresholdNorm, int coverageThresholdNorm)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                using (var bitmap2 = BitmapHelper.CopyToBpp(bitmap, 8))
                {
                    return IsBlankRGB(bitmap2, whiteThresholdNorm, coverageThresholdNorm);
                }
            }
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                return false;
            }
            return IsBlankRGB(bitmap, whiteThresholdNorm, coverageThresholdNorm);
        }

        private static bool IsBlankRGB(Bitmap bitmap, int whiteThresholdNorm, int coverageThresholdNorm)
        {
            var whiteThreshold = (int)Math.Round(WHITE_THRESHOLD_MIN + (whiteThresholdNorm / 100.0) * (WHITE_THRESHOLD_MAX - WHITE_THRESHOLD_MIN));
            var coverageThreshold = COVERAGE_THRESHOLD_MIN + (coverageThresholdNorm / 100.0) * (COVERAGE_THRESHOLD_MAX - COVERAGE_THRESHOLD_MIN);

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
                    // Use standard values for grayscale conversion to weight the RGB values
                    int luma = r * 299 + g * 587 + b * 114;
                    if (luma < whiteThreshold * 1000)
                    {
                        matchPixels++;
                    }
                }
            }

            var coverage = (matchPixels / (double)totalPixels);
            return coverage < coverageThreshold;
        }

        public bool ExcludePage(Bitmap bitmap, ScanProfile scanProfile)
        {
            return scanProfile.ExcludeBlankPages && IsBlank(bitmap, scanProfile.BlankPageWhiteThreshold, scanProfile.BlankPageCoverageThreshold);
        }
    }
}