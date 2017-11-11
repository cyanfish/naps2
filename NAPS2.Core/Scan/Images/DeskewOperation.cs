using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public class DeskewOperation : OperationBase
    {
        private readonly ThreadFactory threadFactory;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageRenderer scannedImageRenderer;

        private volatile bool cancel;
        private Thread thread;

        public DeskewOperation(ThreadFactory threadFactory, ThumbnailRenderer thumbnailRenderer, ScannedImageRenderer scannedImageRenderer)
        {
            this.threadFactory = threadFactory;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageRenderer = scannedImageRenderer;

            AllowCancel = true;
        }

        public bool Start(ICollection<ScannedImage> images)
        {
            ProgressTitle = MiscResources.AutoDeskewProgress;
            Status = new OperationStatus
            {
                StatusText = MiscResources.AutoDeskewing,
                MaxProgress = images.Count
            };
            cancel = false;

            thread = threadFactory.StartThread(() =>
            {
                var memoryLimitingSem = new Semaphore(4, 4);
                Pipeline.For(images).StepParallel(img =>
                {
                    if (cancel)
                    {
                        return null;
                    }
                    memoryLimitingSem.WaitOne();
                    Bitmap bitmap = scannedImageRenderer.Render(img);
                    try
                    {
                        if (cancel)
                        {
                            return null;
                        }
                        var transform = RotationTransform.Auto(bitmap);
                        if (cancel)
                        {
                            return null;
                        }
                        bitmap = transform.Perform(bitmap);
                        img.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));

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
                }).Step((img, transform) =>
                {
                    img.AddTransform(transform);
                    return img;
                }).Run();
                Status.Success = !cancel;
                InvokeFinished();
            });

            return true;
        }

        public override void Cancel()
        {
            cancel = true;
        }

        public override void WaitUntilFinished()
        {
            thread.Join();
        }
    }
}
