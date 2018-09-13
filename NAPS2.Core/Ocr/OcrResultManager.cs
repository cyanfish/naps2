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
        private readonly Dictionary<OcrRequestParams, OcrRequest> requestCache = new Dictionary<OcrRequestParams, OcrRequest>();
        private readonly AutoResetEvent queueWaitHandle = new AutoResetEvent(false);
        private readonly List<Task> workerTasks = new List<Task>();
        private CancellationTokenSource workerCts = new CancellationTokenSource();

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

        public async Task<OcrResult> StartForeground(IOcrEngine ocrEngine, ScannedImage.Snapshot snapshot, string tempImageFilePath, OcrParams ocrParams, CancellationToken cancelToken)
        {
            OcrRequest req;
            lock (this)
            {
                ocrEngine = ocrEngine ?? ocrManager.ActiveEngine ?? throw new ArgumentException("No OCR engine available");
                ocrParams = ocrParams ?? ocrManager.DefaultParams;

                var reqParams = new OcrRequestParams(snapshot, ocrEngine, ocrParams);
                req = requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
                // Fast path for cached results
                if (req.Result != null)
                {
                    File.Delete(tempImageFilePath);
                    return req.Result;
                }
                // Manage ownership of the provided temp file
                if (req.TempImageFilePath == null)
                {
                    req.TempImageFilePath = tempImageFilePath;
                }
                else
                {
                    File.Delete(tempImageFilePath);
                }
                // Increment the reference count
                req.ForegroundCount += 1;
                queueWaitHandle.Set();
            }
            // If no worker threads are running, start them
            EnsureWorkerThreads();
            // Wait for completion or cancellation
            await Task.Factory.StartNew(() => WaitHandle.WaitAny(new[] { req.WaitHandle, cancelToken.WaitHandle }), TaskCreationOptions.LongRunning);
            lock (this)
            {
                // Decrement the reference count
                req.ForegroundCount -= 1;
                // If all requestors have cancelled and there's no result to cache, delete the request
                DestroyRequest(req);
            }
            // If no requests are pending, stop the worker threads
            EnsureWorkerThreads();
            // May return null if cancelled
            return req.Result;
        }

        public void StartBackground(ScannedImage.Snapshot snapshot)
        {
            OcrRequest req;
            CancellationTokenSource cts = new CancellationTokenSource();
            lock (this)
            {
                var ocrEngine = ocrManager.ActiveEngine;
                if (ocrEngine == null) return;
                var ocrParams = ocrManager.DefaultParams;

                var reqParams = new OcrRequestParams(snapshot, ocrEngine, ocrParams);
                req = requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
                // Fast path for cached results
                if (req.Result != null)
                {
                    return;
                }
                // Manage ownership of the provided snapshot
                if (req.Snapshot == null)
                {
                    req.Snapshot = snapshot;
                }
                else
                {
                    snapshot.Dispose();
                }
                // Increment the reference count
                req.BackgroundCount += 1;
                snapshot.Source.ThumbnailInvalidated += (sender, args) => cts.Cancel();
                snapshot.Source.FullyDisposed += (sender, args) => cts.Cancel();
                queueWaitHandle.Set();
            }
            // If no worker threads are running, start them
            EnsureWorkerThreads();
            var op = StartingOne();
            Task.Factory.StartNew(() =>
            {
                WaitHandle.WaitAny(new[] {req.WaitHandle, cts.Token.WaitHandle, op.CancelToken.WaitHandle});
                lock (this)
                {
                    // Decrement the reference count
                    req.BackgroundCount -= 1;
                    // If all requestors have cancelled and there's no result to cache, delete the request
                    DestroyRequest(req);
                }
                FinishedOne();
                // If no requests are pending, stop the worker threads
                EnsureWorkerThreads();
            }, TaskCreationOptions.LongRunning);
        }

        private void DestroyRequest(OcrRequest req)
        {
            if (req.ForegroundCount + req.BackgroundCount == 0)
            {
                if (!req.IsProcessing)
                {
                    req.Snapshot?.Dispose();
                    if (req.TempImageFilePath != null) File.Delete(req.TempImageFilePath);
                }
                if (req.Result == null)
                {
                    req.CancelSource.Cancel();
                    requestCache.Remove(req.Params);
                }
            }
        }

        private void EnsureWorkerThreads()
        {
            lock (this)
            {
                bool hasPending = requestCache.Values.Any(x => x.ForegroundCount + x.BackgroundCount > 0);
                if (workerTasks.Count == 0 && hasPending)
                {
                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        workerTasks.Add(Task.Factory.StartNew(() => RunWorkerTask(workerCts), TaskCreationOptions.LongRunning).Unwrap());
                    }
                }
                if (workerTasks.Count > 0 && !hasPending)
                {
                    workerCts.Cancel();
                    workerTasks.Clear();
                    workerCts = new CancellationTokenSource();
                }
            }
        }

        private async Task RunWorkerTask(CancellationTokenSource cts)
        {
            while (true)
            {
                // Wait for a queued ocr request to become available
                WaitHandle.WaitAny(new[] { queueWaitHandle, cts.Token.WaitHandle });
                if (cts.IsCancellationRequested)
                {
                    return;
                }
                // Get the next queued request
                OcrRequest next;
                string tempImageFilePath;
                lock (this)
                {
                    next = requestCache.Values
                        .OrderByDescending(x => x.ForegroundCount)
                        .ThenByDescending(x => x.BackgroundCount)
                        .FirstOrDefault(x => x.BackgroundCount + x.ForegroundCount > 0 && !x.IsProcessing && x.Result == null);
                    if (next == null)
                    {
                        continue;
                    }
                    next.IsProcessing = true;
                    tempImageFilePath = next.TempImageFilePath;
                }
                // If not already provided, render the image to a file
                if (tempImageFilePath == null)
                {
                    tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
                    using (var bitmap = await renderer.Render(next.Snapshot))
                    {
                        bitmap.Save(tempImageFilePath);
                    }
                }
                // Actually run OCR
                var result = next.Params.Engine.ProcessImage(tempImageFilePath, next.Params.OcrParams, next.CancelSource.Token);
                // Update the request
                lock (this)
                {
                    if (result != null)
                    {
                        next.Result = result;
                    }
                    next.IsProcessing = false;
                    next.WaitHandle.Set();
                }
                // Clean up
                File.Delete(tempImageFilePath);
                next.Snapshot?.Dispose();
            }
        }
        
        private OcrResultOperation StartingOne()
        {
            lock (this)
            {
                if (currentOp == null)
                {
                    currentOp = new OcrResultOperation();
                    operationProgress.ShowBackgroundProgress(currentOp);
                }
                currentOp.IncrementMax();
                return currentOp;
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
