using System.Threading;

namespace NAPS2.Images;

public record PostProcessingData(IMemoryImage? Thumbnail, TransformState? ThumbnailTransformState,
    Barcode Barcode, CancellationTokenSource? OcrCts)
{
    public PostProcessingData() : this(null, null, Barcode.NoDetection, null)
    {
    }
}