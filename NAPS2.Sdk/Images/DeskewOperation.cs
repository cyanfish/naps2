namespace NAPS2.Images;

public class DeskewOperation : OperationBase
{
    private readonly ImageContext _imageContext;
    private readonly ImageRenderer _imageRenderer;

    public DeskewOperation() : this(ImageContext.Default, new ImageRenderer(ImageContext.Default))
    {
    }

    public DeskewOperation(ImageContext imageContext, ImageRenderer imageRenderer)
    {
        _imageContext = imageContext;
        _imageRenderer = imageRenderer;

        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(ICollection<ScannedImage> images, DeskewParams deskewParams)
    {
        ProgressTitle = MiscResources.AutoDeskewProgress;
        Status = new OperationStatus
        {
            StatusText = MiscResources.AutoDeskewing,
            MaxProgress = images.Count
        };

        RunAsync(async () =>
        {
            return await Pipeline.For(images, CancelToken).RunParallel(async img =>
            {
                var bitmap = await _imageRenderer.Render(img);
                try
                {
                    CancelToken.ThrowIfCancellationRequested();
                    var transform = Deskewer.GetDeskewTransform(bitmap);
                    CancelToken.ThrowIfCancellationRequested();
                    bitmap = _imageContext.PerformTransform(bitmap, transform);
                    var thumbnail = deskewParams.ThumbnailSize.HasValue
                        ? _imageContext.PerformTransform(bitmap, new ThumbnailTransform(deskewParams.ThumbnailSize.Value))
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
                    bitmap.Dispose();
                }
            });
        });

        return true;
    }
}