using System.Threading;

namespace NAPS2.Ocr;

/// <summary>
/// Tracks and reports OCR operation progress.
///
/// Call RegisterOcrController with each controller instance that should be included in the operation progress. An
/// instance of OcrOperation is dynamically generated when a page's OCR is queued and OCR isn't already queued or
/// processing for another page. If OCR is already queued, it is added to the total for the existing operation.
///
/// For example, we start with 0/1, then 0/2 when a new page is queued, then 2/2 once completed, then 0/1 if another
/// page is then queued.
/// </summary>
internal class OcrOperationManager
{
    // TODO: Consider implementing/using a MultiDict
    private readonly Dictionary<OcrController, HashSet<Task>> _ongoingTasks = new();
    private readonly OperationProgress _operationProgress;
    private OcrOperation? _currentOp;

    public OcrOperationManager(OperationProgress operationProgress)
    {
        _operationProgress = operationProgress;
    }

    /// <summary>
    /// Registers an OcrController to report progress through the managed OcrOperation.
    /// </summary>
    /// <param name="controller"></param>
    public void RegisterOcrController(OcrController controller)
    {
        controller.OcrStarted += OcrStarted;
        controller.OcrCompleted += OcrCompleted;
    }

    private void OcrStarted(object? sender, OcrEventArgs e)
    {
        OcrOperation op;
        bool newOp = false;
        lock (this)
        {
            if (_currentOp == null)
            {
                _currentOp = new OcrOperation(WaitAll);
                _currentOp.CancelToken.Register(Cancel);
                newOp = true;
            }
            op = _currentOp;
            op.Status.MaxProgress += 1;
            _ongoingTasks.GetOrSet((OcrController) sender!, () => []).Add(e.ResultTask);
        }
        op.InvokeStatusChanged();
        if (newOp)
        {
            _operationProgress.ShowBackgroundProgress(op);
        }
    }

    private void WaitAll(CancellationToken cancelToken)
    {
        Task.WaitAll(_ongoingTasks.Values.SelectMany(x => x).ToArray(), cancelToken);
    }

    private void Cancel()
    {
        foreach (var controller in _ongoingTasks.Keys)
        {
            controller.CancelAll();
        }
    }

    private void OcrCompleted(object? sender, OcrEventArgs e)
    {
        OcrOperation op;
        bool finished = false;
        lock (this)
        {
            op = _currentOp ?? throw new InvalidOperationException();
            _currentOp.Status.CurrentProgress += 1;
            if (_currentOp.Status.CurrentProgress == _currentOp.Status.MaxProgress)
            {
                _currentOp = null;
                finished = true;
            }
            var taskSet = _ongoingTasks[(OcrController) sender!];
            taskSet.Remove(e.ResultTask);
            if (taskSet.Count == 0)
            {
                _ongoingTasks.Remove((OcrController) sender!);
            }
        }
        op.InvokeStatusChanged();
        if (finished)
        {
            op.InvokeFinished();
        }
    }
}