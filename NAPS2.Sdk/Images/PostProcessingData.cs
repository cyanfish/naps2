using System.Threading;

namespace NAPS2.Images;

// TODO: We need to do OCR cancellation when the image is deleted or whatever. 
public record PostProcessingData(IMemoryImage? Thumbnail, TransformState? ThumbnailTransformState,
    BarcodeDetection BarcodeDetection, CancellationTokenSource? OcrCts)
{
    public PostProcessingData() : this(null, null, BarcodeDetection.NotAttempted, null)
    {
    }
}