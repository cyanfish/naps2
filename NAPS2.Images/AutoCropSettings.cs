namespace NAPS2.Images;

/// <summary>
/// How a single axis (width or height) should be handled during auto-cropping.
/// </summary>
public enum AutoCropAxisMode
{
    /// <summary>
    /// The axis is left untouched (no cropping on that axis).
    /// </summary>
    Off,

    /// <summary>
    /// The content edges are detected automatically and the axis is cropped to fit
    /// the detected content (plus padding). This is what you want for the height of
    /// a receipt scanned with an over-long/indefinite scan area: the blank tail is
    /// trimmed back to where the content actually ends.
    /// </summary>
    Auto,

    /// <summary>
    /// The axis is cropped to a fixed size in pixels, anchored on the detected
    /// content. This is what you want for the width of receipts that all share the
    /// same width: every scan is cropped to exactly the same number of pixels.
    /// </summary>
    Fixed
}

/// <summary>
/// Settings controlling <see cref="AutoCropper"/>. Width and height are configured
/// independently, so you can (for example) auto-detect height while forcing a fixed
/// width.
/// </summary>
public record AutoCropSettings
{
    /// <summary>How the horizontal axis (width) is handled. Default: Fixed.</summary>
    public AutoCropAxisMode WidthMode { get; init; } = AutoCropAxisMode.Fixed;

    /// <summary>How the vertical axis (height) is handled. Default: Auto.</summary>
    public AutoCropAxisMode HeightMode { get; init; } = AutoCropAxisMode.Auto;

    /// <summary>
    /// Target width in pixels when <see cref="WidthMode"/> is Fixed. If null, Fixed
    /// behaves like Auto (falls back to the detected content width).
    /// </summary>
    public int? FixedWidthPx { get; init; }

    /// <summary>
    /// Target height in pixels when <see cref="HeightMode"/> is Fixed. If null, Fixed
    /// behaves like Auto (falls back to the detected content height).
    /// </summary>
    public int? FixedHeightPx { get; init; }

    /// <summary>
    /// Extra margin (in pixels) kept around detected content on auto-cropped edges.
    /// </summary>
    public int PaddingPx { get; init; } = 8;

    /// <summary>
    /// A row/column is treated as "content" when the fraction of dark pixels along it
    /// meets or exceeds this value. Keeps isolated specks/noise from being detected as
    /// content. Range 0..1.
    /// </summary>
    public double ThresholdFraction { get; init; } = 0.004;

    /// <summary>
    /// Minimum absolute number of dark pixels for a row/column to count as content,
    /// regardless of <see cref="ThresholdFraction"/>.
    /// </summary>
    public int MinContentPixels { get; init; } = 3;
}
