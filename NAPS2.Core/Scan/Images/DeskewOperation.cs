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

        private bool cancel;
        private Thread thread;

        public DeskewOperation(ThreadFactory threadFactory, ThumbnailRenderer thumbnailRenderer)
        {
            this.threadFactory = threadFactory;
            this.thumbnailRenderer = thumbnailRenderer;

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
                foreach (var img in images)
                {
                    Bitmap bitmap = img.GetImage();
                    try
                    {
                        var transform = RotationTransform.Auto(bitmap, () => cancel);
                        if (cancel)
                        {
                            break;
                        }
                        img.AddTransform(transform);
                        bitmap = transform.Perform(bitmap);
                        img.SetThumbnail(thumbnailRenderer.RenderThumbnail(bitmap));
                        Status.CurrentProgress++;
                        InvokeStatusChanged();
                    }
                    finally
                    {
                        bitmap.Dispose();
                    }
                }
                Status.Success = !cancel;
                InvokeFinished();
            });

            return true;
        }

        public override void Cancel()
        {
            cancel = true;
        }

        public void WaitUntilFinished()
        {
            thread.Join();
        }
    }
}
