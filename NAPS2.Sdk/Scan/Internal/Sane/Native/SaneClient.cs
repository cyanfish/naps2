using System.Runtime.InteropServices;
using System.Threading;

namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneClient : SaneNativeObject
{
    private static readonly object SaneLock = new();

    private static SaneNativeLibrary GetNativeLibrary(ISaneInstallation saneInstallation)
    {
        lock (SaneLock)
        {
            saneInstallation.Initialize();
            return new SaneNativeLibrary(saneInstallation.LibraryPath, saneInstallation.LibraryDeps);
        }
    }

    public SaneClient(ISaneInstallation saneInstallation) : base(GetNativeLibrary(saneInstallation), IntPtr.Zero)
    {
        Monitor.Enter(SaneLock);
        Native.sane_init(out _, IntPtr.Zero);
    }

    public IEnumerable<SaneDeviceInfo> GetDevices()
    {
        HandleStatus(Native.sane_get_devices(out var deviceListPtr, 0));
        IntPtr devicePtr;
        int offset = 0;
        while ((devicePtr = Marshal.ReadIntPtr(deviceListPtr, offset++ * IntPtr.Size)) != IntPtr.Zero)
        {
            var device = Marshal.PtrToStructure<SaneDeviceInfo>(devicePtr);
            yield return device;
        }
    }

    // This calls sane_stream_devices which is not a normal part of SANE and is patched into
    // NAPS2 SANE builds. It will fail when using a SANE installation without the patch.
    public void StreamDevices(Action<SaneDeviceInfo> callback, CancellationToken cancelToken)
    {
        HandleStatus(Native.sane_stream_devices(devicePtr =>
        {
            if (devicePtr != IntPtr.Zero)
            {
                var device = Marshal.PtrToStructure<SaneDeviceInfo>(devicePtr);
                callback(device);
            }
            return cancelToken.IsCancellationRequested ? 0 : 1;
        }, 0));
    }

    public SaneDevice OpenDevice(string deviceName)
    {
        HandleStatus(Native.sane_open(deviceName, out var handle));
        return new SaneDevice(Native, handle);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Native.sane_exit();
        }
        Monitor.Exit(SaneLock);
    }
}