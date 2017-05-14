using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class DirectImportOperation : OperationBase
    {
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ThreadFactory threadFactory;

        private bool cancel;
        private Thread thread;

        public DirectImportOperation(ThumbnailRenderer thumbnailRenderer, ThreadFactory threadFactory)
        {
            this.thumbnailRenderer = thumbnailRenderer;
            this.threadFactory = threadFactory;

            AllowCancel = true;
        }

        public bool Start(DirectImageTransfer data, bool copy, Action<ScannedImage> imageCallback)
        {
            ProgressTitle = copy ? MiscResources.CopyProgress : MiscResources.ImportProgress;
            Status = new OperationStatus
            {
                StatusText = copy ? MiscResources.Copying : MiscResources.Importing,
                MaxProgress = data.ImageRecovery.Length
            };
            cancel = false;

            thread = threadFactory.StartThread(() =>
            {
                Exception error = null;
                foreach (var ir in data.ImageRecovery)
                {
                    try
                    {
                        ScannedImage img;
                        using (var bitmap = new Bitmap(Path.Combine(data.RecoveryFolder, ir.FileName)))
                        {
                            img = new ScannedImage(bitmap, ir.BitDepth, ir.HighQuality, -1);
                        }
                        foreach (var transform in ir.TransformList)
                        {
                            img.AddTransform(transform);
                        }
                        img.SetThumbnail(thumbnailRenderer.RenderThumbnail(img));
                        imageCallback(img);

                        Status.CurrentProgress++;
                        InvokeStatusChanged();
                        if (cancel)
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
                    Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, data.RecoveryFolder), error);
                }
                Status.Success = true;
                InvokeFinished();
            });
            return true;
        }

        public override void WaitUntilFinished()
        {
            thread.Join();
        }

        public override void Cancel()
        {
            cancel = true;
        }
    }
}
