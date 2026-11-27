using System.Runtime.InteropServices;
using NAPS2.Unmanaged;
using NAPS2.Util;
using toff_t = System.IntPtr;
using tsize_t = System.IntPtr;
using thandle_t = System.IntPtr;
using tdata_t = System.IntPtr;

namespace NAPS2.Images.Gtk;

internal static class LibTiff
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    private static readonly Dictionary<Type, object> FuncCache = new();

    private static readonly Lazy<IntPtr> LibraryHandle = new(() =>
    {
        var handle = dlopen("libtiff.so.5", RTLD_LAZY | RTLD_GLOBAL);
        if (handle == IntPtr.Zero)
        {
            handle = dlopen("libtiff.so.6", RTLD_LAZY | RTLD_GLOBAL);
        }
        if (handle == IntPtr.Zero)
        {
            var error = dlerror();
            throw new InvalidOperationException($"Could not load library: \"libtiff\". Error: {error}");
        }
        return handle;
    });

    public static T Load<T>()
    {
        return (T) FuncCache.Get(typeof(T), () => Marshal.GetDelegateForFunctionPointer<T>(LoadFunc<T>())!);
    }

    private static IntPtr LoadFunc<T>()
    {
        var symbol = typeof(T).Name.Split("_")[0];
        var ptr = dlsym(LibraryHandle.Value, symbol);
        if (ptr == IntPtr.Zero)
        {
            var error = dlerror();
            throw new InvalidOperationException($"Could not load symbol: \"{symbol}\". Error: {error}");
        }
        return ptr;
    }

    public delegate IntPtr TIFFOpen_d(string filename, string mode);

    public static TIFFOpen_d TIFFOpen => Load<TIFFOpen_d>();

    public delegate IntPtr TIFFSetErrorHandler_d(TIFFErrorHandler handler);

    public static TIFFSetErrorHandler_d TIFFSetErrorHandler => Load<TIFFSetErrorHandler_d>();

    public delegate IntPtr TIFFSetWarningHandler_d(TIFFErrorHandler handler);

    public static TIFFSetWarningHandler_d TIFFSetWarningHandler => Load<TIFFSetWarningHandler_d>();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TIFFErrorHandler(string x, string y, IntPtr va_args);

    public delegate IntPtr TIFFClientOpen_d(string filename, string mode, IntPtr clientdata,
        TIFFReadWriteProc readproc, TIFFReadWriteProc writeproc, TIFFSeekProc seekproc, TIFFCloseProc closeproc,
        TIFFSizeProc sizeproc, TIFFMapFileProc mapproc, TIFFUnmapFileProc unmapproc);

    public static TIFFClientOpen_d TIFFClientOpen => Load<TIFFClientOpen_d>();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate tsize_t TIFFReadWriteProc(thandle_t clientdata, tdata_t data, tsize_t size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate toff_t TIFFSeekProc(thandle_t clientdata, toff_t off, int c);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int TIFFCloseProc(thandle_t clientdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate toff_t TIFFSizeProc(thandle_t clientdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int TIFFMapFileProc(thandle_t clientdata, ref tdata_t a, ref toff_t b);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TIFFUnmapFileProc(thandle_t clientdata, tdata_t a, toff_t b);

    public delegate IntPtr TIFFClose_d(IntPtr tiff);

    public static TIFFClose_d TIFFClose => Load<TIFFClose_d>();

    public delegate short TIFFNumberOfDirectories_d(IntPtr tiff);

    public static TIFFNumberOfDirectories_d TIFFNumberOfDirectories => Load<TIFFNumberOfDirectories_d>();

    public delegate int TIFFReadDirectory_d(IntPtr tiff);

    public static TIFFReadDirectory_d TIFFReadDirectory => Load<TIFFReadDirectory_d>();

    public delegate int TIFFWriteDirectory_d(IntPtr tiff);

    public static TIFFWriteDirectory_d TIFFWriteDirectory => Load<TIFFWriteDirectory_d>();

    public delegate int TIFFVGetField_d(IntPtr tiff, TiffTag tag, IntPtr ap);

    /// <summary>
    /// Gets a single-valued field. This is only safe if T matches the type libtiff writes for the tag (e.g. short for
    /// uint16_t, int for uint32_t, float for float). Otherwise libtiff can write past the end of the value.
    /// </summary>
    public static unsafe int TIFFGetField<T>(IntPtr tiff, TiffTag tag, out T field) where T : unmanaged
    {
        T value = default;
        int result = VaList.Call(ap => Load<TIFFVGetField_d>()(tiff, tag, ap), (IntPtr) (&value));
        field = value;
        return result;
    }

    /// <summary>
    /// Gets the embedded ICC color profile, or null if there isn't one.
    /// </summary>
    public static unsafe byte[]? GetIccProfile(IntPtr tiff)
    {
        uint count = 0;
        IntPtr data = IntPtr.Zero;
        int result = VaList.Call(ap => Load<TIFFVGetField_d>()(tiff, TiffTag.IccProfile, ap),
            (IntPtr) (&count), (IntPtr) (&data));
        if (result != 1 || count == 0 || data == IntPtr.Zero)
        {
            return null;
        }
        var profile = new byte[count];
        Marshal.Copy(data, profile, 0, (int) count);
        return profile;
    }

    public delegate int TIFFVSetField_d(IntPtr tiff, TiffTag tag, IntPtr ap);

    public static int TIFFSetField(IntPtr tiff, TiffTag tag, params VaArg[] args) =>
        VaList.Call(ap => Load<TIFFVSetField_d>()(tiff, tag, ap), args);

    public delegate int TIFFWriteScanline_d(
        IntPtr tiff, tdata_t buf, int row, short sample);

    public static TIFFWriteScanline_d TIFFWriteScanline => Load<TIFFWriteScanline_d>();

    public delegate int TIFFReadRGBAImage_d(
        IntPtr tiff, int w, int h, IntPtr raster, int stopOnError);

    public static TIFFReadRGBAImage_d TIFFReadRGBAImage => Load<TIFFReadRGBAImage_d>();

    public delegate int TIFFReadRGBAImageOriented_d(
        IntPtr tiff, int w, int h, IntPtr raster, int orientation, int stopOnError);

    public static TIFFReadRGBAImageOriented_d TIFFReadRGBAImageOriented => Load<TIFFReadRGBAImageOriented_d>();

    [DllImport("libdl.so.2")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so.2")]
    public static extern string dlerror();

    [DllImport("libdl.so.2")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);
}