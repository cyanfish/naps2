using NAPS2.ImportExport.Images;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.ImportExport;

public class DirectImportOperation : OperationBase
{
    private readonly ScanningContext _scanningContext;
    private readonly WorkerPool _workerPool;

    public DirectImportOperation(ScanningContext scanningContext, WorkerPool workerPool)
    {
        _scanningContext = scanningContext;
        _workerPool = workerPool;

        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(ImageTransferData data, bool copy, Action<ProcessedImage> imageCallback,
        DirectImportParams importParams)
    {
        ProgressTitle = copy ? MiscResources.CopyProgress : MiscResources.ImportProgress;
        Status = new OperationStatus
        {
            StatusText = copy ? MiscResources.Copying : MiscResources.Importing,
            MaxProgress = data.SerializedImages.Count
        };

        RunAsync(async () =>
        {
            // TODO: Do we want to try and continue after errors?
            // Keep track of image references that haven't been sent to the callback yet so we can dispose them
            // correctly in case of cancellation/error
            using var ownedImages = new DisposableSet<ProcessedImage>();
            try
            {
                // ReSharper disable AccessToDisposedClosure
                await Pipeline
                    .For(data.SerializedImages, CancelToken)
                    .StepParallel(serializedImage =>
                    {
                        var img = ImageSerializer.Deserialize(_scanningContext, serializedImage,
                            new DeserializeImageOptions());
                        ownedImages.Add(img);
                        return img;
                    })
                    .StepParallel(img =>
                    {
                        var thumbnailSize =
                            img.PostProcessingData.Thumbnail == null ? importParams.ThumbnailSize : null;
                        ProcessedImage newImg;
                        try
                        {
                            newImg = _workerPool.Use(
                                WorkerType.Native,
                                ctx =>
                                    ctx.Service.ImportPostProcess(_scanningContext, img, thumbnailSize,
                                        new BarcodeDetectionOptions()));
                        }
                        catch (Exception)
                        {
                            if (!CancelToken.IsCancellationRequested) throw;
                            return null!;
                        }
                        ownedImages.Add(newImg);
                        return newImg;
                    })
                    .Run(img =>
                    {
                        ownedImages.Remove(img);
                        imageCallback(img);

                        Status.CurrentProgress++;
                        InvokeStatusChanged();
                    });
            }
            catch (Exception ex)
            {
                Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, "<data>"), ex);
            }
            return true;
        });
        return true;
    }
}