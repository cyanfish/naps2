namespace NAPS2.Images;

public record PostProcessingData(IMemoryImage? Thumbnail, BarcodeDetection BarcodeDetection)
{
    public PostProcessingData() : this(null, BarcodeDetection.NotAttempted)
    {
    }
}
