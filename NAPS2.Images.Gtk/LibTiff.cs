using System.Runtime.InteropServices;
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

    public delegate int TIFFGetField_d1(IntPtr tiff, TiffTag tag, out int field);

    public static TIFFGetField_d1 TIFFGetFieldInt => Load<TIFFGetField_d1>();

    public delegate int TIFFGetField_d2(IntPtr tiff, TiffTag tag, out float field);

    public static TIFFGetField_d2 TIFFGetFieldFloat => Load<TIFFGetField_d2>();

    public delegate int TIFFGetField_d3(IntPtr tiff, TiffTag tag, out double field);

    public static TIFFGetField_d3 TIFFGetFieldDouble => Load<TIFFGetField_d3>();

    public delegate int TIFFSetField_d1(IntPtr tiff, TiffTag tag, int field);

    public static TIFFSetField_d1 TIFFSetFieldInt => Load<TIFFSetField_d1>();

    public delegate int TIFFSetField_d2(IntPtr tiff, TiffTag tag, float field);

    public static TIFFSetField_d2 TIFFSetFieldFloat => Load<TIFFSetField_d2>();

    public delegate int TIFFSetField_d3(IntPtr tiff, TiffTag tag, double field);

    public static TIFFSetField_d3 TIFFSetFieldDouble => Load<TIFFSetField_d3>();

    public delegate int TIFFSetField_d4(IntPtr tiff, TiffTag tag, short field, short[] array);

    public static TIFFSetField_d4 TIFFSetFieldShortArray => Load<TIFFSetField_d4>();

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