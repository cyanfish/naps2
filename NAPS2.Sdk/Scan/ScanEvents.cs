using NAPS2.Scan.Internal;

namespace NAPS2.Scan;

internal class ScanEvents : IScanEvents
{
    public static readonly IScanEvents Stub = new ScanEvents(() => { }, _ => { }, _ => { });

    private readonly Action _pageStartCallback;
    private readonly Action<double> _pageProgressCallback;
    private readonly Action<string> _connectionUriChangedCallback;

    public ScanEvents(Action pageStartCallback, Action<double> pageProgressCallback,
        Action<string> connectionUriChangedCallback)
    {
        _pageStartCallback = pageStartCallback;
        _pageProgressCallback = pageProgressCallback;
        _connectionUriChangedCallback = connectionUriChangedCallback;
    }

    public void PageStart()
    {
        _pageStartCallback();
    }

    public void PageProgress(double progress)
    {
        _pageProgressCallback(progress);
    }

    public void ConnectionUriChanged(string uri)
    {
        _connectionUriChangedCallback(uri);
    }
}