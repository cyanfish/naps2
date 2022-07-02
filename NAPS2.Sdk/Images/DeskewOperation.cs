using NAPS2.Images.Gdi;

namespace NAPS2.Images;

public class DeskewOperation : OperationBase
{
    private readonly ImageContext _imageContext;

    public DeskewOperation(ImageContext imageContext)
    {
        _imageContext = imageContext;

        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(ICollection<UiImage> images, DeskewParams deskewParams)
    {
        ProgressTitle = MiscResources.AutoDeskewProgress;
        Status = new OperationStatus
        {
            StatusText = MiscResources.AutoDeskewing,
            MaxProgress = images.Count
        };

        RunAsync(async () =>
        {
            return await Pipeline.For(images, CancelToken).RunParallel(img =>
            {
                using var processedImage = img.GetClonedImage();
                var image = _imageContext.Render(processedImage);
                try
                {
                    CancelToken.ThrowIfCancellationRequested();
                    var transform = Deskewer.GetDeskewTransform(image);
                    CancelToken.ThrowIfCancellationRequested();
                    image = _imageContext.PerformTransform(image, transform);
                    var thumbnail = deskewParams.ThumbnailSize.HasValue
                        ? _imageContext.PerformTransform(image, new ThumbnailTransform(deskewParams.ThumbnailSize.Value))
                        : null;
                    lock (img)
                    {
                        img.AddTransform(transform);
                        if (thumbnail != null)
                        {
                            img.SetThumbnail(thumbnail);
                        }
                    }
                    lock (this)
                    {
                        Status.CurrentProgress += 1;
                    }
                    InvokeStatusChanged();
                }
                finally
                {
                    image.Dispose();
                }
            });
        });

        return true;
    }
}