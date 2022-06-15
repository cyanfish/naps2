using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.ImportExport;

public class DirectImportOperation : OperationBase
{
    private readonly ScanningContext _scanningContext;

    public DirectImportOperation(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;

        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(ImageTransferData data, bool copy, Action<ProcessedImage> imageCallback, DirectImportParams importParams)
    {
        ProgressTitle = copy ? MiscResources.CopyProgress : MiscResources.ImportProgress;
        Status = new OperationStatus
        {
            StatusText = copy ? MiscResources.Copying : MiscResources.Importing,
            MaxProgress = data.SerializedImages.Count
        };

        RunAsync(async () =>
        {
            Exception? error = null;
            foreach (var serializedImage in data.SerializedImages)
            {
                try
                {
                    ProcessedImage img = SerializedImageHelper.Deserialize(_scanningContext, serializedImage, new SerializedImageHelper.DeserializeOptions());
                    // TODO: Don't bother, here, in recovery, etc.
                    if (img.PostProcessingData.Thumbnail == null && importParams.ThumbnailSize.HasValue)
                    {
                        var renderedImage = _scanningContext.ImageContext.Render(img);
                        var thumbnail = _scanningContext.ImageContext.PerformTransform(renderedImage,
                            new ThumbnailTransform(importParams.ThumbnailSize.Value));
                        img = img.WithPostProcessingData(img.PostProcessingData with
                        {
                            Thumbnail = thumbnail
                        }, true);
                    }
                    imageCallback(img);

                    Status.CurrentProgress++;
                    InvokeStatusChanged();
                    if (CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }
            if (error != null)
            {
                Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, "<data>"), error);
            }
            return true;
        });
        return true;
    }
}