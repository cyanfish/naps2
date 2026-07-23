using NAPS2.Images;

namespace NAPS2.Config;

/// <summary>
/// Persisted user settings for the Auto Crop feature. Width and height are configured
/// independently (e.g. fixed width for same-width receipts, auto-detected height for
/// over-long receipt scans).
/// </summary>
public record AutoCropConfig
{
    public AutoCropAxisMode WidthMode { get; init; } = AutoCropAxisMode.Fixed;

    public AutoCropAxisMode HeightMode { get; init; } = AutoCropAxisMode.Auto;

    public double? FixedWidthMm { get; init; }

    public double? FixedHeightMm { get; init; }

    public double PaddingMm { get; init; } = 2.0;
}
