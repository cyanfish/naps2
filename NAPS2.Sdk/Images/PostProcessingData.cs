using System.Threading;

namespace NAPS2.Images;

/// <summary>
/// Represents information about an image obtained during post-processing (e.g. thumbnail image, barcode).
/// </summary>
public record PostProcessingData(
    IMemoryImage? Thumbnail,
    TransformState? ThumbnailTransformState,
    int PageNumber,
    PageSide PageSide,
    Barcode Barcode,
    CancellationTokenSource? OcrCts)
{
    public PostProcessingData() : this(null, null, 0, PageSide.Unknown, Barcode.NoDetection, null)
    {
    }
}