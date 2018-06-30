﻿using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NAPS2.ImportExport
{
    public class ImportOperation : OperationBase
    {
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly ThreadFactory threadFactory;

        private bool cancel;
        private Thread thread;

        public ImportOperation(IScannedImageImporter scannedImageImporter, ThreadFactory threadFactory)
        {
            this.scannedImageImporter = scannedImageImporter;
            this.threadFactory = threadFactory;

            ProgressTitle = MiscResources.ImportProgress;
            AllowCancel = true;
        }

        public bool Start(List<string> filesToImport, Action<ScannedImage> imageCallback)
        {
            bool oneFile = filesToImport.Count == 1;
            Status = new OperationStatus
            {
                MaxProgress = oneFile ? 0 : filesToImport.Count
            };
            cancel = false;

            thread = threadFactory.StartThread(() =>
            {
                Run(filesToImport, imageCallback, oneFile);
                GC.Collect();
                InvokeFinished();
            });
            return true;
        }

        private void Run(IEnumerable<string> filesToImport, Action<ScannedImage> imageCallback, bool oneFile)
        {
            foreach (var fileName in filesToImport)
            {
                try
                {
                    Status.StatusText = string.Format(MiscResources.ImportingFormat, Path.GetFileName(fileName));
                    InvokeStatusChanged();
                    var images = scannedImageImporter.Import(fileName, new ImportParams(), (i, j) =>
                    {
                        if (oneFile)
                        {
                            Status.CurrentProgress = i;
                            Status.MaxProgress = j;
                            InvokeStatusChanged();
                        }
                        return !cancel;
                    });
                    foreach (var img in images)
                    {
                        imageCallback(img);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)), ex);
                    InvokeError(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)), ex);
                }
                if (!oneFile)
                {
                    Status.CurrentProgress++;
                    InvokeStatusChanged();
                }
            }
            Status.Success = true;
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