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