using System.Threading;

namespace NAPS2.Ocr;

internal class OcrRequest
{
    private readonly TaskCompletionSource<OcrResult?> _tcs = new();
    private readonly CancellationTokenSource _requestCts = new();
    private string? _tempImageFilePath;
    private int _activeReferences = 0;

    public OcrRequest(OcrRequestParams reqParams)
    {
        Params = reqParams;
    }

    public OcrRequestParams Params { get; }

    public void AddReference(string tempImageFilePath, OcrPriority priority, CancellationToken cancelToken)
    {
        var referencePriority = priority == OcrPriority.Foreground ? 100 : 1;
        bool referenceAdded = false;
        lock (this)
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
        lock (this)
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
        lock (this)
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
        lock (this)
        {
            if (State != OcrRequestState.Processing)
            {
                throw new InvalidOperationException();
            }
        }
        OcrResult? result = null;
        try
        {
            result = await Params.Engine.ProcessImage(_tempImageFilePath!, Params.OcrParams, _requestCts.Token);
        }
        catch (Exception e)
        {
            Log.ErrorException("Error in OcrEngine.ProcessImage", e);
        }
        SafeDelete(_tempImageFilePath!);
        lock (this)
        {
            State = result != null ? OcrRequestState.Completed : OcrRequestState.Error;
        }
        _tcs.SetResult(result);
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
}