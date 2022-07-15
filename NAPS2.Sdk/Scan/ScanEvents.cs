using NAPS2.Scan.Internal;

namespace NAPS2.Scan;

internal class ScanEvents : IScanEvents
{
    public static readonly IScanEvents Stub = new ScanEvents(() => { }, _ => { });

    private readonly Action _pageStartCallback;
    private readonly Action<double> _pageProgressCallback;

    public ScanEvents(Action pageStartCallback, Action<double> pageProgressCallback)
    {
        _pageStartCallback = pageStartCallback;
        _pageProgressCallback = pageProgressCallback;
    }

    public void PageStart()
    {
        _pageStartCallback();
    }

    public void PageProgress(double progress)
    {
        _pageProgressCallback(progress);
    }
}