using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Ocr;

/// <summary>
/// State and processing logic for OCR requests that go through OcrRequestQueue.
/// </summary>
internal class OcrRequest
{
    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;
    private readonly OcrRequestQueue _ocrRequestQueue;
    private readonly TaskCompletionSource<OcrResult?> _tcs = new();
    private readonly CancellationTokenSource _requestCts = new();
    private string? _tempImageFilePath;
    private int _activeReferences = 0;

    public OcrRequest(ScanningContext scanningContext, OcrRequestQueue ocrRequestQueue, OcrRequestParams reqParams)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
        _ocrRequestQueue = ocrRequestQueue;
        Params = reqParams;
    }

    public OcrRequestParams Params { get; }

    public void AddReference(string tempImageFilePath, OcrPriority priority, CancellationToken cancelToken)
    {
        var referencePriority = priority == OcrPriority.Foreground ? 100 : 1;
        bool referenceAdded = false;
        lock (_ocrRequestQueue)
        {
            if (_tempImageFilePath != null)
            {
                SafeDelete(tempImageFilePath);
            }
            else
            {
                _tempImageFilePath = tempImageFilePath;
            }
            if (State == OcrRequestState.Pending)
            {
                RequestPriority += referencePriority;
                _activeReferences += 1;
                referenceAdded = true;
            }
        }
        if (referenceAdded)
        {
            // Run continuations outside the lock
            cancelToken.Register(() => RemoveReference(referencePriority));
        }
    }

    private void RemoveReference(int referencePriority)
    {
        bool requestCanceled = false;
        lock (_ocrRequestQueue)
        {
            RequestPriority -= referencePriority;
            _activeReferences -= 1;
            if (_activeReferences == 0 && State is OcrRequestState.Pending or OcrRequestState.Processing)
            {
                State = OcrRequestState.Canceled;
                requestCanceled = true;
            }
        }
        if (requestCanceled)
        {
            // Run continuations outside the lock
            _requestCts.Cancel();
            _tcs.SetResult(null);
        }
    }

    public Task<OcrResult?> CompletedTask => _tcs.Task;

    public int RequestPriority { get; private set; }

    public OcrRequestState State { get; private set; } = OcrRequestState.Pending;

    public void MoveToProcessingState()
    {
        lock (_ocrRequestQueue)
        {
            if (State != OcrRequestState.Pending)
            {
                throw new InvalidOperationException();
            }
            State = OcrRequestState.Processing;
        }
    }

    public async Task Process()
    {
        lock (_ocrRequestQueue)
        {
            if (State != OcrRequestState.Processing)
            {
                throw new InvalidOperationException();
            }
        }
        OcrResult? result = null;
        try
        {
            result = await Params.Engine.ProcessImage(_scanningContext, _tempImageFilePath!, Params.OcrParams, _requestCts.Token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in OcrEngine.ProcessImage");
        }
        SafeDelete(_tempImageFilePath!);
        lock (_ocrRequestQueue)
        {
            State = result != null ? OcrRequestState.Completed : OcrRequestState.Error;
        }
        _tcs.SetResult(result);
    }

    private void SafeDelete(string path)
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
            _logger.LogError(e, "Error deleting temp OCR file");
        }
    }
}