using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class ImportOperation : OperationBase
    {
        private readonly IScannedImageImporter scannedImageImporter;

        public ImportOperation(IScannedImageImporter scannedImageImporter)
        {
            this.scannedImageImporter = scannedImageImporter;

            ProgressTitle = MiscResources.ImportProgress;
            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(List<string> filesToImport, Action<ScannedImage> imageCallback)
        {
            bool oneFile = filesToImport.Count == 1;
            Status = new OperationStatus
            {
                MaxProgress = oneFile ? 0 : filesToImport.Count
            };

            RunAsync(async () =>
            {
                try
                {
                    foreach (var fileName in filesToImport)
                    {
                        try
                        {
                            Status.StatusText = string.Format(MiscResources.ImportingFormat, Path.GetFileName(fileName));
                            InvokeStatusChanged();
                            var imageSrc = scannedImageImporter.Import(fileName, new ImportParams(), oneFile ? OnProgress : new ProgressHandler((j, k) => { }), CancelToken);
                            await imageSrc.ForEach(imageCallback);
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
                    return true;
                }
                finally
                {
                    GC.Collect();
                }
            });
            return true;
        }
    }
}
