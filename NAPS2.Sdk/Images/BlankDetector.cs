using System.Runtime.InteropServices;
using NAPS2.Images.Gdi;

namespace NAPS2.Images;

// TODO: Add tests
public static class BlankDetector
{
    // If the pixel value (0-255) >= white_threshold, then it counts as a white pixel.
    private const int WHITE_THRESHOLD_MIN = 1;
    private const int WHITE_THRESHOLD_MAX = 255;
    // If the fraction of non-white pixels > coverage_threshold, then it counts as a non-blank page.
    private const double COVERAGE_THRESHOLD_MIN = 0.00;
    private const double COVERAGE_THRESHOLD_MAX = 0.01;

    public static bool IsBlank(IMemoryImage image, int whiteThresholdNorm, int coverageThresholdNorm)
    {
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            using var bitmap2 = image.PerformTransform(new ColorBitDepthTransform());
            return IsBlankRGB(bitmap2, whiteThresholdNorm, coverageThresholdNorm);
        }
        if (image.PixelFormat != ImagePixelFormat.RGB24)
        {
            return false;
        }
        return IsBlankRGB(image, whiteThresholdNorm, coverageThresholdNorm);
    }

    private static bool IsBlankRGB(IMemoryImage image, int whiteThresholdNorm, int coverageThresholdNorm)
    {
        var whiteThreshold = (int)Math.Round(WHITE_THRESHOLD_MIN + (whiteThresholdNorm / 100.0) * (WHITE_THRESHOLD_MAX - WHITE_THRESHOLD_MIN));
        var coverageThreshold = COVERAGE_THRESHOLD_MIN + (coverageThresholdNorm / 100.0) * (COVERAGE_THRESHOLD_MAX - COVERAGE_THRESHOLD_MIN);

        long totalPixels = image.Width * image.Height;
        long matchPixels = 0;

        // TODO: Wrap this in a bitwise op
        using var lockState = image.Lock(LockMode.ReadOnly, out var data);
        var bytes = new byte[data.stride * image.Height];
        Marshal.Copy(data.safePtr, bytes, 0, bytes.Length);
        lockState.Dispose();
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                int b = bytes[data.stride * y + x * 3];
                int g = bytes[data.stride * y + x * 3 + 1];
                int r = bytes[data.stride * y + x * 3 + 2];
                // Use standard values for grayscale conversion to weight the RGB values
                int luma = r * 299 + g * 587 + b * 114;
                if (luma < whiteThreshold * 1000)
                {
                    matchPixels++;
                }
            }
        }

        var coverage = matchPixels / (double)totalPixels;
        return coverage < coverageThreshold;
    }
}