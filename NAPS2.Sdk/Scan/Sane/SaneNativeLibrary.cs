// ReSharper disable InconsistentNaming

namespace NAPS2.Scan.Sane;

public class SaneNativeLibrary : Unmanaged.NativeLibrary
{
    private static readonly Lazy<SaneNativeLibrary> LazyInstance = new(() =>
    {
        var testRoot = Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT");
        var libraryPath = FindPath(PlatformCompat.System.SaneLibraryName, testRoot);
        var nativeLib = new SaneNativeLibrary(libraryPath);
        nativeLib.sane_init(out _, IntPtr.Zero);
        return nativeLib;
    });

    public static SaneNativeLibrary Instance => LazyInstance.Value;

    private SaneNativeLibrary(string libraryPath)
        : base(libraryPath)
    {
    }

    ~SaneNativeLibrary()
    {
        // TODO: Is this safe? Maybe just do this in a better way, e.g. if we do sane in a worker process, call this
        // when the worker finishes
        sane_exit();
    }

    public delegate SANE_Status sane_init_delegate(out int version_code, IntPtr authorize);
    public delegate void sane_exit_delegate();
    public delegate SANE_Status sane_get_devices_delegate(out IntPtr device_list, int local_only);
    public delegate SANE_Status sane_open_delegate(string name, out IntPtr handle);
    public delegate void sane_close_delegate(IntPtr handle);
    public delegate ref SANE_Option_Descriptor sane_get_option_descriptor_delegate(IntPtr handle, int n);
    public delegate SANE_Status sane_control_option_delegate(IntPtr handle, int n, int a, IntPtr v, out int i);
    public delegate SANE_Status sane_get_parameters_delegate(IntPtr handle, out SANE_Parameters p);
    public delegate SANE_Status sane_start_delegate(IntPtr handle);
    public delegate SANE_Status sane_read_delegate(IntPtr handle, byte[] buf, int maxlen, out int len);
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

    public struct SANE_Option_Descriptor
    {
        public string name;
        public string title;
        public string desc;
        public int type;
        public int unit;
        public int size;
        public int cap;
        public int constraint_type;
        public IntPtr constraint;
    }

    public struct SANE_Parameters
    {
        public SANE_Frame frame;
        public int last_frame;
        public int bytes_per_line;
        public int pixels_per_line;
        public int lines;
        public int depth;
    }

    public struct SANE_Device
    {
        public string name;
        public string vendor;
        public string model;
        public string type;
    }

    public enum SANE_Frame
    {
        Gray = 0,
        Rgb = 1,
        Red = 2,
        Green = 3,
        Blue = 4
    }

    public enum SANE_Status
    {
        Good = 0,
        Unsupported = 1,
        Cancelled = 2,
        DeviceBusy = 3,
        Invalid = 4,
        Eof = 5,
        Jammed = 6,
        NoDocs = 7,
        CoverOpen = 8,
        IoError = 9,
        NoMem = 10,
        AccessDenied = 11
    }
}