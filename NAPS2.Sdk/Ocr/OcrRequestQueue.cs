using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public class OcrRequestQueue
    {
        private static OcrRequestQueue _default;

        public static OcrRequestQueue Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default ?? (_default = new OcrRequestQueue());
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        private readonly Dictionary<OcrRequestParams, OcrRequest> _requestCache = new Dictionary<OcrRequestParams, OcrRequest>();
        private readonly Semaphore _queueWaitHandle = new Semaphore(0, int.MaxValue);
        private List<Task> _workerTasks = new List<Task>();
        private CancellationTokenSource _workerCts = new CancellationTokenSource();

        private readonly OcrEngineManager _ocrEngineManager;
        private readonly OperationProgress _operationProgress;

        private OcrOperation? _currentOp;

        public OcrRequestQueue() : this(OcrEngineManager.Default, OperationProgress.Default)
        {
        }

        public OcrRequestQueue(OcrEngineManager ocrEngineManager, OperationProgress operationProgress)
        {
            _ocrEngineManager = ocrEngineManager;
            _operationProgress = operationProgress;
        }

        public bool HasCachedResult(IOcrEngine ocrEngine, ScannedImage.Snapshot snapshot, OcrParams ocrParams)
        {
            var reqParams = new OcrRequestParams(snapshot, ocrEngine, ocrParams);
            lock (this)
            {
                return _requestCache.ContainsKey(reqParams) && _requestCache[reqParams].Result != null;
            }
        }

        public async Task<OcrResult?> QueueForeground(IOcrEngine ocrEngine, ScannedImage.Snapshot snapshot, string tempImageFilePath, OcrParams ocrParams, CancellationToken cancelToken)
        {
            OcrRequest req;
            lock (this)
            {
                ocrEngine = ocrEngine ?? _ocrEngineManager.ActiveEngine ?? throw new ArgumentException("No OCR engine available");

                var reqParams = new OcrRequestParams(snapshot, ocrEngine, ocrParams);
                req = _requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
                // Fast path for cached results
                if (req.Result != null)
                {
                    SafeDelete(tempImageFilePath);
                    return req.Result;
                }
                // Manage ownership of the provided temp file
                if (req.TempImageFilePath == null)
                {
                    req.TempImageFilePath = tempImageFilePath;
                }
                else
                {
                    SafeDelete(tempImageFilePath);
                }
                // Increment the reference count
                req.ForegroundCount += 1;
                _queueWaitHandle.Release();
            }
            // If no worker threads are running, start them
            EnsureWorkerThreads();
            // Wait for completion or cancellation
            await Task.Run(() =>
            {
                try
                {
                    WaitHandle.WaitAny(new[] { req.WaitHandle, cancelToken.WaitHandle });
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error in OcrRequestQueue.QueueForeground response task", e);
                }
            });
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

        public void QueueBackground(ScannedImage.Snapshot snapshot, string tempImageFilePath, OcrParams ocrParams)
        {
            OcrRequest req;
            CancellationTokenSource cts = new CancellationTokenSource();
            lock (this)
            {
                var ocrEngine = _ocrEngineManager.ActiveEngine;
                if (ocrEngine == null) return;

                var reqParams = new OcrRequestParams(snapshot, ocrEngine, ocrParams);
                req = _requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
                // Fast path for cached results
                if (req.Result != null)
                {
                    return;
                }
                // Manage ownership of the provided temp file
                if (req.TempImageFilePath == null)
                {
                    req.TempImageFilePath = tempImageFilePath;
                }
                else
                {
                    SafeDelete(tempImageFilePath);
                }
                // Increment the reference count
                req.BackgroundCount += 1;
                snapshot.Source.ThumbnailInvalidated += (sender, args) => cts.Cancel();
                snapshot.Source.FullyDisposed += (sender, args) => cts.Cancel();
                _queueWaitHandle.Release();
            }
            // If no worker threads are running, start them
            EnsureWorkerThreads();
            var op = StartingOne();
            Task.Run(() =>
            {
                try
                {
                    WaitHandle.WaitAny(new[] { req.WaitHandle, cts.Token.WaitHandle, op.CancelToken.WaitHandle });
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
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error in OcrRequestQueue.QueueBackground response task", e);
                }
            });
        }

        private void DestroyRequest(OcrRequest req)
        {
            if (req.ForegroundCount + req.BackgroundCount == 0)
            {
                if (!req.IsProcessing)
                {
                    SafeDelete(req.TempImageFilePath);
                }
                if (req.Result == null)
                {
                    req.CancelSource.Cancel();
                    if (_requestCache.Get(req.Params) == req) _requestCache.Remove(req.Params);
                }
            }
        }

        private static void SafeDelete(string path)
        {
            try
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Log.ErrorException("Error deleting temp OCR file", e);
            }
        }

        private void EnsureWorkerThreads()
        {
            lock (this)
            {
                bool hasPending = _requestCache.Values.Any(x => x.ForegroundCount + x.BackgroundCount > 0);
                if (_workerTasks.Count == 0 && hasPending)
                {
                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        _workerTasks.Add(Task.Run(() => RunWorkerTask(_workerCts)));
                    }
                }
                if (_workerTasks.Count > 0 && !hasPending)
                {
                    _workerCts.Cancel();
                    _workerTasks = new List<Task>();
                    _workerCts = new CancellationTokenSource();
                }
            }
        }

        private void RunWorkerTask(CancellationTokenSource cts)
        {
            try
            {
                while (true)
                {
                    // Wait for a queued ocr request to become available
                    WaitHandle.WaitAny(new[] { _queueWaitHandle, cts.Token.WaitHandle });
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    // Get the next queued request
                    OcrRequest next;
                    string tempImageFilePath;
                    lock (this)
                    {
                        next = _requestCache.Values
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

                    // Actually run OCR
                    var result = next.Params.Engine.ProcessImage(tempImageFilePath, next.Params.OcrParams, next.CancelSource.Token);
                    // Update the request
                    lock (this)
                    {
                        if (result != null)
                        {
                            next.Result = result;
                        }

                        if (next.Result == null)
                        {
                            if (_requestCache.Get(next.Params) == next) _requestCache.Remove(next.Params);
                        }

                        next.IsProcessing = false;
                        next.WaitHandle.Set();
                    }

                    // Clean up
                    SafeDelete(tempImageFilePath);
                }
            }
            catch (Exception e)
            {
                Log.ErrorException("Error in OcrRequestQueue.RunWorkerTask", e);
            }
        }

        private OcrOperation StartingOne()
        {
            OcrOperation op;
            bool started = false;
            lock (this)
            {
                if (_currentOp == null)
                {
                    _currentOp = new OcrOperation(_workerTasks);
                    started = true;
                }
                op = _currentOp;
                op.Status.MaxProgress += 1;
            }
            op.InvokeStatusChanged();
            if (started)
            {
                _operationProgress.ShowBackgroundProgress(op);
            }
            return op;
        }

        private void FinishedOne()
        {
            OcrOperation op;
            bool finished = false;
            lock (this)
            {
                op = _currentOp;
                _currentOp.Status.CurrentProgress += 1;
                if (_currentOp.Status.CurrentProgress == _currentOp.Status.MaxProgress)
                {
                    _currentOp = null;
                    finished = true;
                }
            }
            op.InvokeStatusChanged();
            if (finished)
            {
                op.InvokeFinished();
            }
        }
    }
}
