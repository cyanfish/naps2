using System.Collections.Generic;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class DeskewOperation : OperationBase
    {
        private readonly ImageContext imageContext;
        private readonly ImageRenderer imageRenderer;

        public DeskewOperation() : this(ImageContext.Default, new ImageRenderer(ImageContext.Default))
        {
        }

        public DeskewOperation(ImageContext imageContext, ImageRenderer imageRenderer)
        {
            this.imageContext = imageContext;
            this.imageRenderer = imageRenderer;

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
                    var bitmap = await imageRenderer.Render(img);
                    try
                    {
                        CancelToken.ThrowIfCancellationRequested();
                        var transform = Deskewer.GetDeskewTransform(bitmap);
                        CancelToken.ThrowIfCancellationRequested();
                        bitmap = imageContext.PerformTransform(bitmap, transform);
                        var thumbnail = deskewParams.ThumbnailSize.HasValue
                            ? imageContext.PerformTransform(bitmap, new ThumbnailTransform(deskewParams.ThumbnailSize.Value))
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
}
