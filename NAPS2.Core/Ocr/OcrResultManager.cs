using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public class OcrResultManager
    {
        private readonly Dictionary<ScannedImage, Task<OcrResult>> taskMap = new Dictionary<ScannedImage, Task<OcrResult>>();
        private readonly Semaphore semaphore = new Semaphore(2, 2); // TODO: Set in the constructor based on the engine

        private readonly OcrManager ocrManager;
        private readonly ScannedImageRenderer renderer;
        private readonly IOperationProgress operationProgress;

        private OcrResultOperation currentOp;

        public OcrResultManager(OcrManager ocrManager, ScannedImageRenderer renderer, IOperationProgress operationProgress)
        {
            this.ocrManager = ocrManager;
            this.renderer = renderer;
            this.operationProgress = operationProgress;
        }

        public Task<OcrResult> GetTask(ScannedImage image)
        {
            lock (this)
            {
                return taskMap.GetOrSet(image, () => RunOcr(image));
            }
        }

        private Task<OcrResult> RunOcr(ScannedImage image)
        {
            StartingOne();
            return Task.Factory.StartNew(async () =>
            {
                semaphore.WaitOne();
                try
                {
                    using (var snapshot = image.Preserve())
                    {
                        string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                        using (var bitmap = await renderer.Render(snapshot))
                        {
                            bitmap.Save(tempImageFilePath);
                        }
                        try
                        {
                            // TODO: Also cancel on image disposal
                            return ocrManager.ActiveEngine?.ProcessImage(tempImageFilePath, ocrManager.DefaultParams, () => currentOp.CancelToken.IsCancellationRequested);
                        }
                        finally
                        {
                            File.Delete(tempImageFilePath);
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                    FinishedOne();
                }
            }, TaskCreationOptions.LongRunning).Unwrap();
        }

        private void StartingOne()
        {   
            lock (this)
            {
                if (currentOp == null)
                {
                    currentOp = new OcrResultOperation();
                    operationProgress.ShowBackgroundProgress(currentOp);
                }
                currentOp.IncrementMax();
            }
        }

        private void FinishedOne()
        {
            lock (this)
            {
                currentOp.IncrementCurrent();
                if (currentOp.Status.CurrentProgress == currentOp.Status.MaxProgress)
                {
                    currentOp.Finish();
                    currentOp = null;
                }
            }
        }
    }
}
