#if MAC
using System.Collections.Immutable;
using ImageCaptureCore;

namespace NAPS2.Scan.Internal.Apple;

internal class DeviceReader : ICDeviceBrowserDelegate
{
    private readonly ICDeviceBrowser _browser = new();

    public DeviceReader()
    {
        _browser.Delegate = this;
        _browser.BrowsedDeviceTypeMask =
            ICBrowsedDeviceType.Scanner | ICBrowsedDeviceType.Local | ICBrowsedDeviceType.Remote;
    }

    public ImmutableList<ICScannerDevice> Devices { get; private set; } = ImmutableList<ICScannerDevice>.Empty;

    public void Start()
    {
        _browser.Start();
    }

    public event EventHandler<DeviceEventArgs>? DeviceFound;

    public override void DidAddDevice(ICDeviceBrowser browser, ICDevice device, bool moreComing)
    {
        // TODO: Use moreComing
        if (device.Type.HasFlag(ICDeviceType.Scanner))
        {
            var scannerDevice = (ICScannerDevice) device;
            Devices = Devices.Add(scannerDevice);
            DeviceFound?.Invoke(this, new DeviceEventArgs(scannerDevice));
        }
    }

    public override void DidRemoveDevice(ICDeviceBrowser browser, ICDevice device, bool moreGoing)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // _browser.Stop();
            // _browser.Dispose();
        }
        base.Dispose(disposing);
    }

    internal record DeviceEventArgs(ICScannerDevice Device);
}
#endif