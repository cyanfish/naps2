namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneDevice : SaneNativeObject
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

    public SaneReadParameters GetParameters()
    {
        HandleStatus(Native.sane_get_parameters(Handle, out var p));
        return p;
    }

    public bool Read(byte[] buffer, out int len)
    {
        var status = Native.sane_read(Handle, buffer, buffer.Length, out len);
        if (status == SaneStatus.Good)
        {
            return true;
        }
        if (status is SaneStatus.Eof or SaneStatus.NoDocs)
        {
            return false;
        }
        HandleStatus(status);
        return false;
    }
}