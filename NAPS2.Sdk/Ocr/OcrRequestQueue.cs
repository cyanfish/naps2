using System.Threading;

namespace NAPS2.Ocr;

public class OcrRequestQueue
{
    private static OcrRequestQueue? _default;

    public static OcrRequestQueue Default
    {
        get
        {
            TestingContext.NoStaticDefaults();
            return _default ??= new OcrRequestQueue();
        }
        set => _default = value ?? throw new ArgumentNullException(nameof(value));
    }

    private readonly Dictionary<OcrRequestParams, OcrRequest> _requestCache = new Dictionary<OcrRequestParams, OcrRequest>();
    private readonly Semaphore _queueWaitHandle = new Semaphore(0, int.MaxValue);
    private List<Task> _workerTasks = new List<Task>();
    private CancellationTokenSource _workerCts = new CancellationTokenSource();

    private readonly OperationProgress _operationProgress;

    public OcrRequestQueue() : this(OperationProgress.Default)
    {
    }

    public OcrRequestQueue(OperationProgress operationProgress)
    {
        _operationProgress = operationProgress;
    }

    public bool HasCachedResult(IOcrEngine ocrEngine, ProcessedImage image, OcrParams ocrParams)
    {
        var reqParams = new OcrRequestParams(image, ocrEngine, ocrParams);
        lock (this)
        {
            return _requestCache.ContainsKey(reqParams) && _requestCache[reqParams].Result != null;
        }
    }

    public async Task<OcrResult?> Enqueue(IOcrEngine ocrEngine, ProcessedImage image, string tempImageFilePath, OcrParams ocrParams, OcrPriority priority, CancellationToken cancelToken)
    {
        OcrRequest req;
        lock (this)
        {
            var reqParams = new OcrRequestParams(image, ocrEngine, ocrParams);
            req = _requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
            // Fast path for cached results
            if (req.Result != null)
            {
                // TODO: We didn't do this in background ocr before, is there any problem with this?
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
            req.PriorityRefCount[priority] += 1;
            _queueWaitHandle.Release();
        }
        // If no worker threads are running, start them
        EnsureWorkerThreads();
        // Wait for completion or cancellation
        await Task.Run(() => WaitHandle.WaitAny(new[] { req.WaitHandle, cancelToken.WaitHandle }));
        lock (this)
        {
            req.PriorityRefCount[priority] -= 1;
            // If all requestors have cancelled and there's no result to cache, delete the request
            MaybeGarbageCollectRequest(req);
        }
        // If no requests are pending, stop the worker threads
        EnsureWorkerThreads();
        // May return null if cancelled
        return req.Result;
    }

    private void MaybeGarbageCollectRequest(OcrRequest req)
    {
        if (!req.HasLiveReference)
        {
            // If the OCR engine is already processing this request, then it will clean up the temp file when it's
            // done anyway.
            // TODO: There's still a race case where we delete it here just before processing starts. Can we handle
            // that or simplify if handling isn't necessary?
            if (!req.IsProcessing)
            {
                SafeDelete(req.TempImageFilePath ?? throw new InvalidOperationException());
            }
            if (req.Result == null)
            {
                // TODO: What does this do? Is it needed?
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
            bool hasPending = _requestCache.Values.Any(x => x.HasLiveReference);
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
                OcrRequest? next;
                string tempImageFilePath;
                lock (this)
                {
                    next = _requestCache.Values
                        .OrderByDescending(x => x.PriorityRefCount[OcrPriority.Foreground])
                        .ThenByDescending(x => x.PriorityRefCount[OcrPriority.Background])
                        .FirstOrDefault(x => x.HasLiveReference && !x.IsProcessing && x.Result == null);
                    if (next == null)
                    {
                        continue;
                    }

                    next.IsProcessing = true;
                    tempImageFilePath = next.TempImageFilePath ?? throw new InvalidOperationException();
                }

                // Actually run OCR
                OcrResult? result = null;
                try
                {
                    result = next.Params.Engine.ProcessImage(
                        tempImageFilePath, next.Params.OcrParams, next.CancelSource.Token);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error in OcrEngine.ProcessImage", e);
                }
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
}