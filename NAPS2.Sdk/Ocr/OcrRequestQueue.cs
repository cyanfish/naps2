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

    private readonly Dictionary<OcrRequestParams, OcrRequest> _requestCache = new();
    private Semaphore _queueWaitHandle = new(0, int.MaxValue);
    private List<Task> _workerTasks = new();
    private CancellationTokenSource _workerCts = new();

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
            return _requestCache.ContainsKey(reqParams) && _requestCache[reqParams].State == OcrRequestState.Completed;
        }
    }

    public async Task<OcrResult?> Enqueue(IOcrEngine ocrEngine, ProcessedImage image, string tempImageFilePath,
        OcrParams ocrParams, OcrPriority priority, CancellationToken cancelToken)
    {
        OcrRequest req;
        lock (this)
        {
            var reqParams = new OcrRequestParams(image, ocrEngine, ocrParams);
            req = _requestCache.GetOrSet(reqParams, () => new OcrRequest(reqParams));
            if (req.State is OcrRequestState.Canceled or OcrRequestState.Error)
            {
                // Retry with a new request
                req = _requestCache[reqParams] = new OcrRequest(reqParams);
            }
            req.AddReference(tempImageFilePath, priority, cancelToken);
        }
        if (req.State == OcrRequestState.Completed)
        {
            return await req.CompletedTask;
        }
        // Signal the worker tasks that a request may be ready
        _queueWaitHandle.Release();
        // If no worker threads are running, start them
        EnsureWorkerThreads();
        var result = await req.CompletedTask;
        // If no requests are pending, stop the worker threads
        EnsureWorkerThreads();
        // May return null if cancelled
        return result;
    }

    private void EnsureWorkerThreads()
    {
        // TODO: Maybe it makes sense to have a single dispatcher that creates subtasks for dequeued requests (with a
        // TODO: semaphore to limit concurrency) rather than worker threads/tasks
        lock (this)
        {
            bool hasPending = _requestCache.Values.Any(x => x.State == OcrRequestState.Pending);
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
                _queueWaitHandle = new Semaphore(0, int.MaxValue);
            }
        }
    }

    private async Task RunWorkerTask(CancellationTokenSource cts)
    {
        try
        {
            while (true)
            {
                // Wait for a queued ocr request to become available
                await Task.WhenAny(_queueWaitHandle.WaitOneAsync(), cts.Token.WaitHandle.WaitOneAsync());
                if (cts.IsCancellationRequested)
                {
                    return;
                }

                // Get the next queued request
                OcrRequest? next;
                lock (this)
                {
                    next = _requestCache.Values
                        .OrderByDescending(x => x.RequestPriority)
                        .FirstOrDefault(x => x.State == OcrRequestState.Pending);
                    if (next == null)
                    {
                        continue;
                    }
                    next.MoveToProcessingState();
                }
                await next.Process();
            }
        }
        catch (Exception e)
        {
            Log.ErrorException("Error in OcrRequestQueue.RunWorkerTask", e);
        }
    }
}