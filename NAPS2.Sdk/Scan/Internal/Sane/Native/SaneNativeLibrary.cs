// ReSharper disable InconsistentNaming

namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneNativeLibrary : Unmanaged.NativeLibrary
{
    private static readonly Lazy<SaneNativeLibrary> LazyInstance = new(() =>
    {
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var libraryPath = FindPath(PlatformCompat.System.SaneLibraryName, testRoot);
        var nativeLib = new SaneNativeLibrary(libraryPath);
        return nativeLib;
    });

    public static SaneNativeLibrary Instance => LazyInstance.Value;

    private SaneNativeLibrary(string libraryPath)
        : base(libraryPath)
    {
    }

    public delegate SaneStatus sane_init_delegate(out int version_code, IntPtr authorize);
    public delegate void sane_exit_delegate();
    public delegate SaneStatus sane_get_devices_delegate(out IntPtr device_list, int local_only);
    public delegate SaneStatus sane_open_delegate(string name, out IntPtr handle);
    public delegate void sane_close_delegate(IntPtr handle);
    public delegate ref SaneOptionDescriptor sane_get_option_descriptor_delegate(IntPtr handle, int n);
    public delegate SaneStatus sane_control_option_delegate(IntPtr handle, int n, int a, IntPtr v, out int i);
    public delegate SaneStatus sane_get_parameters_delegate(IntPtr handle, out SaneReadParameters p);
    public delegate SaneStatus sane_start_delegate(IntPtr handle);
    public delegate SaneStatus sane_read_delegate(IntPtr handle, byte[] buf, int maxlen, out int len);
    public delegate void sane_cancel_delegate(IntPtr handle);

    public sane_init_delegate sane_init => Load<sane_init_delegate>();
    public sane_exit_delegate sane_exit => Load<sane_exit_delegate>();
    public sane_get_devices_delegate sane_get_devices => Load<sane_get_devices_delegate>();
    public sane_open_delegate sane_open => Load<sane_open_delegate>();
    public sane_close_delegate sane_close => Load<sane_close_delegate>();
    public sane_get_option_descriptor_delegate sane_get_option_descriptor => Load<sane_get_option_descriptor_delegate>();
    public sane_control_option_delegate sane_control_option => Load<sane_control_option_delegate>();
    public sane_get_parameters_delegate sane_get_parameters => Load<sane_get_parameters_delegate>();
    public sane_start_delegate sane_start => Load<sane_start_delegate>();
    public sane_read_delegate sane_read => Load<sane_read_delegate>();
    public sane_cancel_delegate sane_cancel => Load<sane_cancel_delegate>();
}