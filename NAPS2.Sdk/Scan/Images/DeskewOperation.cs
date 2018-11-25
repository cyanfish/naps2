using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public class DeskewOperation : OperationBase
    {
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        public DeskewOperation(ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;

            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(ICollection<ScannedImage> images)
        {
            ProgressTitle = MiscResources.AutoDeskewProgress;
            Status = new OperationStatus
            {
                StatusText = MiscResources.AutoDeskewing,
                MaxProgress = images.Count
            };

            RunAsync(() =>
            {
                var memoryLimitingSem = new Semaphore(4, 4);
                Pipeline.For(images).StepParallel(img =>
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    memoryLimitingSem.WaitOne();
                    Bitmap bitmap = scannedImageRenderer.Render(img).Result;
                    try
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        var transform = RotationTransform.Auto(bitmap);
                        if (CancelToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        bitmap = transform.Perform(bitmap);
                        var thumbnail = thumbnailRenderer.RenderThumbnail(bitmap);
                        lock (img)
                        {
                            img.AddTransform(transform);
                            img.SetThumbnail(thumbnail);
                        }

                        // The final pipeline step is pretty fast, so updating progress here is more accurate
                        lock (this)
                        {
                            Status.CurrentProgress += 1;
                        }
                        InvokeStatusChanged();

                        return Tuple.Create(img, transform);
                    }
                    finally
                    {
                        bitmap.Dispose();
                        memoryLimitingSem.Release();
                    }
                }).Run();
                return !CancelToken.IsCancellationRequested;
            });

            return true;
        }
    }
}
