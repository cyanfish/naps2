using NAPS2.Scan.Internal;

namespace NAPS2.Scan;

internal class ScanEvents : IScanEvents
{
    public static readonly IScanEvents Stub = new ScanEvents(() => { }, _ => { }, (_, _) => { });

    private readonly Action _pageStartCallback;
    private readonly Action<double> _pageProgressCallback;
    private readonly Action<string?, string?> _deviceUriChangedCallback;

    public ScanEvents(Action pageStartCallback, Action<double> pageProgressCallback,
        Action<string?, string?> deviceUriChangedCallback)
    {
        _pageStartCallback = pageStartCallback;
        _pageProgressCallback = pageProgressCallback;
        _deviceUriChangedCallback = deviceUriChangedCallback;
    }

    public void PageStart()
    {
        _pageStartCallback();
    }

    public void PageProgress(double progress)
    {
        _pageProgressCallback(progress);
    }

    public void DeviceUriChanged(string? iconUri, string? connectionUri)
    {
        _deviceUriChangedCallback(iconUri, connectionUri);
    }
}