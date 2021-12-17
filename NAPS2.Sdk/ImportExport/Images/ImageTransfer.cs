using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Serialization;

namespace NAPS2.ImportExport.Images;

public class ImageTransfer : TransferHelper<IEnumerable<ScannedImage>, ImageTransferData>
{
    private readonly ImageContext _imageContext;

    public ImageTransfer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    protected override ImageTransferData AsData(IEnumerable<ScannedImage> images)
    {
        var transfer = new ImageTransferData
        {
            ProcessId = Process.GetCurrentProcess().Id
        };
        var serializedImages = images.Select(x => SerializedImageHelper.Serialize(_imageContext, (ScannedImage) x, new SerializedImageHelper.SerializeOptions()));
        transfer.SerializedImages.AddRange(serializedImages);
        return transfer;
    }
}