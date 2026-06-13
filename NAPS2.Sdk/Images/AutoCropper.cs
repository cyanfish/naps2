using NAPS2.Images.Bitwise;
using NAPS2.Images.Transforms;

namespace NAPS2.Images;

/// <summary>
/// Detects the content bounds of a scanned image and produces a <see cref="CropTransform"/>.
///
/// The main use case is receipts (or other documents) scanned with an over-long /
/// indefinite scan height: the height is detected automatically and the blank tail is
/// trimmed back to the actual content, while the width can be forced to a fixed size so
/// that a batch of same-width receipts all come out identical.
///
/// Content detection treats sufficiently dark pixels as "content" (the same luma
/// threshold used elsewhere in NAPS2). It therefore assumes a light scanner background,
/// which is the normal case for sheet-fed/flatbed receipt scanning.
/// </summary>
internal static class AutoCropper
{
    public static CropTransform? GetCropTransform(IMemoryImage image, AutoCropSettings settings)
    {
        if (settings.WidthMode == AutoCropAxisMode.Off && settings.HeightMode == AutoCropAxisMode.Off)
        {
            return null;
        }

        var bounds = DetectContentBounds(image, settings);
        if (bounds == null)
        {
            // No content found (e.g. a blank page); don't crop.
            return null;
        }

        var (contentLeft, contentTop, contentRight, contentBottom) = bounds.Value;
        int w = image.Width;
        int h = image.Height;

        var (left, right) =
            ResolveAxis(settings.WidthMode, settings.FixedWidthPx, contentLeft, contentRight, w, settings.PaddingPx);
        var (top, bottom) =
            ResolveAxis(settings.HeightMode, settings.FixedHeightPx, contentTop, contentBottom, h, settings.PaddingPx);

        var transform = new CropTransform(left, right, top, bottom, w, h);
        return transform.IsNull ? null : transform;
    }

    /// <summary>
    /// Resolves the amount to remove from the two edges of a single axis.
    /// Returns (removeFromStart, removeFromEnd) in pixels.
    /// </summary>
    private static (int start, int end) ResolveAxis(
        AutoCropAxisMode mode, int? fixedSize, int contentStart, int contentEnd, int total, int padding)
    {
        if (mode == AutoCropAxisMode.Off)
        {
            return (0, 0);
        }

        if (mode == AutoCropAxisMode.Auto || fixedSize is not { } size || size <= 0 || size >= total)
        {
            // Auto: crop to detected content plus padding (clamped to the image).
            int start = Math.Max(0, contentStart - padding);
            int end = Math.Max(0, total - 1 - contentEnd - padding);
            return (start, end);
        }

        // Fixed: produce a window of exactly `size` pixels, centred on the detected
        // content, clamped so it stays inside the image.
        int center = (contentStart + contentEnd) / 2;
        int windowStart = center - size / 2;
        windowStart = Math.Max(0, Math.Min(windowStart, total - size));
        int removeStart = windowStart;
        int removeEnd = total - (windowStart + size);
        return (removeStart, removeEnd);
    }

    /// <summary>
    /// Finds the bounding box of dark content. Returns (left, top, right, bottom) in
    /// inclusive pixel coordinates, or null if no content meets the threshold.
    /// </summary>
    private static (int left, int top, int right, int bottom)? DetectContentBounds(
        IMemoryImage image, AutoCropSettings settings)
    {
        using var reader = new BitPixelReader(image);
        int w = image.Width;
        int h = image.Height;

        int rowThreshold = Math.Max(settings.MinContentPixels, (int)(w * settings.ThresholdFraction));
        int colThreshold = Math.Max(settings.MinContentPixels, (int)(h * settings.ThresholdFraction));

        var colCounts = new int[w];

        int top = -1;
        int bottom = -1;
        for (int y = 0; y < h; y++)
        {
            int rowCount = 0;
            for (int x = 0; x < w; x++)
            {
                if (reader[y, x])
                {
                    rowCount++;
                    colCounts[x]++;
                }
            }
            if (rowCount >= rowThreshold)
            {
                if (top == -1)
                {
                    top = y;
                }
                bottom = y;
            }
        }

        if (top == -1)
        {
            return null;
        }

        int left = -1;
        int right = -1;
        for (int x = 0; x < w; x++)
        {
            if (colCounts[x] >= colThreshold)
            {
                if (left == -1)
                {
                    left = x;
                }
                right = x;
            }
        }

        if (left == -1)
        {
            // Vertical content exists but no column passed the (stricter, height-based)
            // column threshold; fall back to the full width.
            left = 0;
            right = w - 1;
        }

        return (left, top, right, bottom);
    }
}
