namespace NAPS2.Scan.Sane;

public class SaneDevice : NativeSaneObject
{
    public SaneDevice(IntPtr handle) : base(handle)
    {
    }

    protected override void DisposeHandle()
    {
        Native.sane_close(Handle);
    }

    public void Cancel()
    {
        Native.sane_cancel(Handle);
    }

    public void Start()
    {
        HandleStatus(Native.sane_start(Handle));
    }

    public SaneNativeLibrary.SANE_Parameters GetParameters()
    {
        HandleStatus(Native.sane_get_parameters(Handle, out var p));
        return p;
    }

    public bool Read(byte[] buffer, out int len)
    {
        var status = Native.sane_read(Handle, buffer, buffer.Length, out len);
        if (status == SaneNativeLibrary.SANE_Status.Good)
        {
            return true;
        }
        if (status is SaneNativeLibrary.SANE_Status.Eof or SaneNativeLibrary.SANE_Status.NoDocs)
        {
            return false;
        }
        HandleStatus(status);
        return false;
    }
}