using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Scan.Images.Storage;

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

        public bool IsBlank(IMemoryStorage bitmap, int whiteThresholdNorm, int coverageThresholdNorm)
        {
            if (bitmap.PixelFormat == StoragePixelFormat.BW1)
            {
                // TODO: Make more generic
                if (!(bitmap is GdiStorage gdiStorage))
                {
                    throw new InvalidOperationException("Patch code detection only supported for GdiStorage");
                }
                using (var bitmap2 = BitmapHelper.CopyToBpp(gdiStorage.Bitmap, 8))
                {
                    return IsBlankRGB(new GdiStorage(bitmap2), whiteThresholdNorm, coverageThresholdNorm);
                }
            }
            if (bitmap.PixelFormat != StoragePixelFormat.RGB24)
            {
                return false;
            }
            return IsBlankRGB(bitmap, whiteThresholdNorm, coverageThresholdNorm);
        }

        private static bool IsBlankRGB(IMemoryStorage bitmap, int whiteThresholdNorm, int coverageThresholdNorm)
        {
            var whiteThreshold = (int)Math.Round(WHITE_THRESHOLD_MIN + (whiteThresholdNorm / 100.0) * (WHITE_THRESHOLD_MAX - WHITE_THRESHOLD_MIN));
            var coverageThreshold = COVERAGE_THRESHOLD_MIN + (coverageThresholdNorm / 100.0) * (COVERAGE_THRESHOLD_MAX - COVERAGE_THRESHOLD_MIN);

            long totalPixels = bitmap.Width * bitmap.Height;
            long matchPixels = 0;

            var data = bitmap.Lock(out var scan0, out var stride);
            var bytes = new byte[stride * bitmap.Height];
            Marshal.Copy(scan0, bytes, 0, bytes.Length);
            bitmap.Unlock(data);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
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

        public bool ExcludePage(IMemoryStorage bitmap, ScanProfile scanProfile)
        {
            return scanProfile.ExcludeBlankPages && IsBlank(bitmap, scanProfile.BlankPageWhiteThreshold, scanProfile.BlankPageCoverageThreshold);
        }
    }
}