using System.Threading;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Scan;

public class ScanOperation : OperationBase
{
    private int _pageNumber = 1;

    public ScanOperation(ScanOptions options)
    {
        ProgressTitle = options.Device!.Name;
        Status = new OperationStatus
        {
            StatusText = options.PaperSource == PaperSource.Flatbed
                ? MiscResources.AcquiringData
                : string.Format(MiscResources.ScanProgressPage, _pageNumber),
            MaxProgress = 1000,
            ProgressType = OperationProgressType.BarOnly,
            IndeterminateProgress = options.Driver == Driver.Twain && !TwainProgressEstimator.HasTimingInfo(options)
        };
        AllowBackground = true;
        AllowCancel = true;
    }

    public new CancellationToken CancelToken => base.CancelToken;

    public void Progress(int current, int total)
    {
        if (current > 0 && total > 0)
        {
            Status.IndeterminateProgress = false;
        }
        Status.CurrentProgress = current;
        Status.MaxProgress = total;
        InvokeStatusChanged();
    }

    public void NextPage(int newPageNumber)
    {
        _pageNumber = newPageNumber;
        Status.StatusText = string.Format(MiscResources.ScanProgressPage, _pageNumber);
        InvokeStatusChanged();
    }

    public void Completed()
    {
        InvokeFinished();
    }
}