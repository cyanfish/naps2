using System.Threading;
using NAPS2.Lang.Resources;
using NAPS2.Operation;

namespace NAPS2.Scan;

public class ScanOperation : OperationBase
{
    private int _pageNumber = 1;

    public ScanOperation(ScanDevice device, PaperSource paperSource)
    {
        ProgressTitle = device.Name;
        Status = new OperationStatus
        {
            StatusText = paperSource == PaperSource.Flatbed
                ? MiscResources.AcquiringData
                : string.Format(MiscResources.ScanProgressPage, _pageNumber),
            MaxProgress = 1000,
            ProgressType = OperationProgressType.BarOnly
        };
        AllowBackground = true;
        AllowCancel = true;
    }

    public new CancellationToken CancelToken => base.CancelToken;

    public void Progress(int current, int total)
    {
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