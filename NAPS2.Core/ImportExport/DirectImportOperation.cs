using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ImportExport
{
    public class DirectImportOperation : OperationBase
    {
        private readonly IScannedImageFactory scannedImageFactory;
        private readonly ThreadFactory threadFactory;

        private bool cancel;
        private Thread thread;

        public DirectImportOperation(IScannedImageFactory scannedImageFactory, ThreadFactory threadFactory)
        {
            this.scannedImageFactory = scannedImageFactory;
            this.threadFactory = threadFactory;

            ProgressTitle = MiscResources.ImportProgress;
            AllowCancel = true;
        }

        public bool Start(DirectImageTransfer data, Action<IScannedImage> imageCallback)
        {
            Status = new OperationStatus
            {
                MaxProgress = data.ImageRecovery.Length
            };
            cancel = false;

            thread = threadFactory.StartThread(() =>
            {
                try
                {
                    foreach (var ir in data.ImageRecovery)
                    {
                        using (var bitmap = new Bitmap(Path.Combine(data.RecoveryFolder, ir.FileName)))
                        {
                            var img = scannedImageFactory.Create(bitmap, ir.BitDepth, ir.HighQuality);
                            imageCallback(img);

                            Status.CurrentProgress++;
                            InvokeStatusChanged();
                            if (cancel)
                            {
                                break;
                            }
                        }
                    }
                    Status.Success = true;
                }
                catch (Exception ex)
                {
                    Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, data.RecoveryFolder), ex);
                }
                InvokeFinished();
            });
            return true;
        }

        public void WaitUntilFinished()
        {
            thread.Join();
        }

        public override void Cancel()
        {
            cancel = true;
        }
    }
}
