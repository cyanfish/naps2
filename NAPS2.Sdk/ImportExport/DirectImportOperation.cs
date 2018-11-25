using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class DirectImportOperation : OperationBase
    {
        private readonly ThumbnailRenderer thumbnailRenderer;

        public DirectImportOperation(ThumbnailRenderer thumbnailRenderer)
        {
            this.thumbnailRenderer = thumbnailRenderer;

            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(DirectImageTransfer data, bool copy, Action<ScannedImage> imageCallback)
        {
            ProgressTitle = copy ? MiscResources.CopyProgress : MiscResources.ImportProgress;
            Status = new OperationStatus
            {
                StatusText = copy ? MiscResources.Copying : MiscResources.Importing,
                MaxProgress = data.ImageRecovery.Length
            };

            RunAsync(async () =>
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
                        // TODO: Don't bother, here, in recovery, etc.
                        img.SetThumbnail(await thumbnailRenderer.RenderThumbnail(img));
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
                    Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, data.RecoveryFolder), error);
                }
                return true;
            });
            return true;
        }
    }
}
