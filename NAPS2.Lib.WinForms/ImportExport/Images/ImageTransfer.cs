using NAPS2.Serialization;

namespace NAPS2.ImportExport.Images;

public class ImageTransfer : TransferHelper<IEnumerable<ProcessedImage>, ImageTransferData>
{
    private readonly ImageContext _imageContext;

    public ImageTransfer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    protected override ImageTransferData AsData(IEnumerable<ProcessedImage> images)
    {
        var transfer = new ImageTransferData
        {
            ProcessId = Process.GetCurrentProcess().Id
        };
        var serializedImages = images.Select(x => ImageSerializer.Serialize(x, new SerializeImageOptions()));
        transfer.SerializedImages.AddRange(serializedImages);
        return transfer;
    }
}