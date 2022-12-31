using System.Threading;

namespace NAPS2.Images;

public record PostProcessingData(IMemoryImage? Thumbnail, TransformState? ThumbnailTransformState,
    BarcodeDetection BarcodeDetection, CancellationTokenSource? OcrCts)
{
    public PostProcessingData() : this(null, null, BarcodeDetection.NotAttempted, null)
    {
    }
}