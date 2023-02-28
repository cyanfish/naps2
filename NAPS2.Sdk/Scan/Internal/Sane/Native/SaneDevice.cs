using System.Runtime.InteropServices;

namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneDevice : SaneNativeObject, ISaneDevice
{
    public SaneDevice(SaneNativeLibrary native, IntPtr handle) : base(native, handle)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Native.sane_close(Handle);
        }
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

    public IEnumerable<SaneOption> GetOptions()
    {
        IntPtr ptr;
        int i = 0;
        // Skip option 0 as it's just the option count
        while ((ptr = Native.sane_get_option_descriptor(Handle, ++i)) != IntPtr.Zero)
        {
            yield return new SaneOption(Marshal.PtrToStructure<SaneOptionDescriptor>(ptr), i);
        }
    }

    public unsafe void SetOption(SaneOption option, double value, out SaneOptionSetInfo info)
    {
        int word = option.Type == SaneValueType.Fixed ? SaneFixedPoint.ToFixed(value) : (int) value;
        int* ptr = &word;
        HandleStatus(Native.sane_control_option(Handle, option.Index, SaneOptionAction.SetValue, (IntPtr) ptr, out info));
    }

    public unsafe void GetOption(SaneOption option, out double value)
    {
        int word = 0;
        int* ptr = &word;
        HandleStatus(Native.sane_control_option(Handle, option.Index, SaneOptionAction.GetValue, (IntPtr) ptr, out _));
        value = option.Type == SaneValueType.Fixed ? SaneFixedPoint.ToDouble(word) : word;
    }

    public void SetOption(SaneOption option, string value, out SaneOptionSetInfo info)
    {
        var s = Marshal.StringToHGlobalAnsi(value);
        try
        {
            HandleStatus(Native.sane_control_option(Handle, option.Index, SaneOptionAction.SetValue, s, out info));
        }
        finally
        {
            Marshal.FreeHGlobal(s);
        }
    }
}