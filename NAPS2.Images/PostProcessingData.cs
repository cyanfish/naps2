namespace NAPS2.Images;

public class PostProcessingData
{
    // TODO: Lifetime?
    public IImage? Thumbnail { get; set; }
    // TODO: Once C# 11 gets here, check for nulls in setter
    public BarcodeDetection BarcodeDetection { get; set; } = BarcodeDetection.NotAttempted;
}
