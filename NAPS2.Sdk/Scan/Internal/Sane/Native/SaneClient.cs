using System.Runtime.InteropServices;

namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneClient : SaneNativeObject
{
    public SaneClient() : base(IntPtr.Zero)
    {
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
        return new SaneDevice(handle);
    }

    protected override void Dispose(bool disposing)
    {
        Native.sane_exit();
    }
}