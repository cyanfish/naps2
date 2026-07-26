namespace NAPS2.Images;

/// <summary>
/// User-facing parameters for <see cref="AutoCropOperation"/>. Sizes are expressed in
/// millimetres and converted to pixels using each image's scan resolution (DPI).
/// </summary>
public class AutoCropParams
{
    /// <summary>How the width is handled. Default: Fixed (receipts share a width).</summary>
    public AutoCropAxisMode WidthMode { get; set; } = AutoCropAxisMode.Fixed;

    /// <summary>How the height is handled. Default: Auto (varies per receipt).</summary>
    public AutoCropAxisMode HeightMode { get; set; } = AutoCropAxisMode.Auto;

    /// <summary>Target width in millimetres when <see cref="WidthMode"/> is Fixed.</summary>
    public double? FixedWidthMm { get; set; }

    /// <summary>Target height in millimetres when <see cref="HeightMode"/> is Fixed.</summary>
    public double? FixedHeightMm { get; set; }

    /// <summary>Margin in millimetres kept around detected content on auto edges.</summary>
    public double PaddingMm { get; set; } = 2.0;

    public int? ThumbnailSize { get; set; }

    private static int? MmToPx(double? mm, float dpi)
    {
        if (mm is not { } value || value <= 0 || dpi <= 0)
        {
            return null;
        }
        return (int)Math.Round(value / 25.4 * dpi);
    }

    /// <summary>
    /// Builds the per-image pixel-based settings consumed by the core auto-cropper,
    /// using the given image's resolution to convert millimetres to pixels.
    /// </summary>
    public AutoCropSettings ToSettings(float xDpi, float yDpi)
    {
        // Use a sensible fallback DPI for padding when resolution is unknown so that the
        // padding is still a reasonable absolute size rather than zero.
        float padXDpi = xDpi > 0 ? xDpi : 300;
        float padYDpi = yDpi > 0 ? yDpi : 300;
        int paddingPx = Math.Max(0, MmToPx(PaddingMm, Math.Min(padXDpi, padYDpi)) ?? 0);

        return new AutoCropSettings
        {
            WidthMode = WidthMode,
            HeightMode = HeightMode,
            FixedWidthPx = MmToPx(FixedWidthMm, xDpi),
            FixedHeightPx = MmToPx(FixedHeightMm, yDpi),
            PaddingPx = paddingPx
        };
    }
}
