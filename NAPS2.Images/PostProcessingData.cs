using System.Threading;

namespace NAPS2.Images;

public record PostProcessingData(IMemoryImage? Thumbnail, BarcodeDetection BarcodeDetection, CancellationTokenSource? OcrCts)
{
    public PostProcessingData() : this(null, BarcodeDetection.NotAttempted, null)
    {
    }
}
