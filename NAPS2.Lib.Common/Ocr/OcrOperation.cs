using System.Threading;

namespace NAPS2.Ocr;

public class OcrOperation : OperationBase
{
    private readonly Action<CancellationToken> _waitAll;

    public OcrOperation(Action<CancellationToken> waitAll)
    {
        _waitAll = waitAll;
        ProgressTitle = MiscResources.OcrProgress;
        AllowBackground = true;
        AllowCancel = true;
        SkipExitPrompt = true;
        Status = new OperationStatus
        {
            StatusText = MiscResources.RunningOcr,
            IndeterminateProgress = true
        };
    }

    public override void Wait(CancellationToken cancelToken)
    {
        _waitAll(cancelToken);
    }

    public new CancellationToken CancelToken => base.CancelToken;

    public new void InvokeStatusChanged() => base.InvokeStatusChanged();

    public new void InvokeFinished() => base.InvokeFinished();
}