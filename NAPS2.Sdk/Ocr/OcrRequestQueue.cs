using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Ocr;

// TODO: Might need to use configureawait(false) everywhere, or at least on non-worker threads...
/// <summary>
/// Allows OCR requests to be queued and prioritized. Results are cached so that requests with the same set of
/// parameters (image, engine, language code, etc.) don't do duplicate work.
/// </summary>
public class OcrRequestQueue
{
    private readonly Dictionary<OcrRequestParams, OcrRequest> _requestCache = new();
    private Semaphore _queueWaitHandle = new(0, int.MaxValue);
    private List<Task> _workerTasks = new();
    private CancellationTokenSource _workerCts = new();

    /// <summary>
    /// Gets or sets the number of queue workers, which determines the maximum number of OCR requests that can process
    /// in parallel.
    /// </summary>
    public int WorkerCount { get; init; } = Environment.ProcessorCount;

    /// <summary>
    /// For testing. Adds a delay to the worker tasks to process requests.
    /// </summary>
    public int WorkerAddedLatency { get; set; }

    /// <summary>
    /// Returns true if a previous queued request with the provided parameters has already completed and produced a
    /// result.
    /// </summary>
    public bool HasCachedResult(IOcrEngine ocrEngine, ProcessedImage image, OcrParams ocrParams)
    {
        var reqParams = new OcrRequestParams(image.GetWeakReference(), ocrEngine, ocrParams);
        lock (this)
        {
            return _requestCache.ContainsKey(reqParams) && _requestCache[reqParams].State == OcrRequestState.Completed;
        }
    }

    /// <summary>
    /// Adds a new OCR request to the queue. Before calling this method, the image should be saved to disk and the file
    /// path specified as "tempImageFilePath". The file will automatically be deleted once it is no longer needed for
    /// the OCR request.
    /// </summary>
    /// <param name="scanningContext">The scanning context object.</param>
    /// <param name="ocrEngine">The engine to run.</param>
    /// <param name="image">The image to OCR.</param>
    /// <param name="tempImageFilePath">The on-disk image file path.</param>
    /// <param name="ocrParams">The OCR config parameters.</param>
    /// <param name="priority">The priority of the request.</param>
    /// <param name="cancelToken">A cancellation token.</param>
    /// <returns>The result of the OCR operation, or null if an error occurred (e.g. engine misconfigured).</returns>
    public async Task<OcrResult?> Enqueue(ScanningContext scanningContext, IOcrEngine ocrEngine, ProcessedImage image,
        string tempImageFilePath, OcrParams ocrParams, OcrPriority priority, CancellationToken cancelToken)
    {
        OcrRequest req;
        lock (this)
        {
            var reqParams = new OcrRequestParams(image.GetWeakReference(), ocrEngine, ocrParams);
            req = _requestCache.GetOrSet(reqParams, () => new OcrRequest(scanningContext, this, reqParams));
            if (req.State is OcrRequestState.Canceled or OcrRequestState.Error)
            {
                // Retry with a new request
                req = _requestCache[reqParams] = new OcrRequest(scanningContext, this, reqParams);
            }
            req.AddReference(tempImageFilePath, priority, cancelToken);
        }
        // Signal the worker tasks that a request may be ready
        _queueWaitHandle.Release();
        // If no worker threads are running, start them
        EnsureWorkerThreads(scanningContext);
        await Task.WhenAny(req.CompletedTask, cancelToken.WaitHandle.WaitOneAsync());
        // If no requests are pending, stop the worker threads
        EnsureWorkerThreads(scanningContext);
        // Return null if canceled
        return req.CompletedTask.IsCompleted ? req.CompletedTask.Result : null;
    }

    private void EnsureWorkerThreads(ScanningContext scanningContext)
    {
        lock (this)
        {
            bool hasPending = _requestCache.Values.Any(x => x.State == OcrRequestState.Pending);
            if (_workerTasks.Count == 0 && hasPending)
            {
                for (int i = 0; i < WorkerCount; i++)
                {
                    _workerTasks.Add(Task.Run(() => RunWorkerTask(scanningContext, _workerCts, _queueWaitHandle)));
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

    private async Task RunWorkerTask(ScanningContext scanningContext, CancellationTokenSource cts, WaitHandle queueWaitHandle)
    {
        var logger = scanningContext.Logger;
        try
        {
            if (WorkerAddedLatency > 0)
            {
                await Task.Delay(WorkerAddedLatency);
            }
            while (true)
            {
                // Wait for a queued ocr request to become available
                await Task.WhenAny(queueWaitHandle.WaitOneAsync(), cts.Token.WaitHandle.WaitOneAsync());
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
            logger.LogError(e, "Error in OcrRequestQueue.RunWorkerTask");
        }
    }
}