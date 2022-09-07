using System.Runtime.InteropServices;

namespace NAPS2.Scan.Sane;

public class SaneClient : NativeSaneObject
{
    public SaneClient() : base(IntPtr.Zero)
    {
        Native.sane_init(out _, IntPtr.Zero);
    }

    public IEnumerable<SaneNativeLibrary.SANE_Device> GetDevices()
    {
        HandleStatus(Native.sane_get_devices(out var deviceListPtr, 0));
        IntPtr devicePtr;
        int offset = 0;
        while ((devicePtr = Marshal.ReadIntPtr(deviceListPtr, offset++ * IntPtr.Size)) != IntPtr.Zero)
        {
            var device = Marshal.PtrToStructure<SaneNativeLibrary.SANE_Device>(devicePtr);
            yield return device;
        }
    }

    public SaneDevice OpenDevice(string deviceId)
    {
        HandleStatus(Native.sane_open(deviceId, out var handle));
        return new SaneDevice(handle);
    }

    protected override void DisposeHandle()
    {
        Native.sane_exit();
    }
}