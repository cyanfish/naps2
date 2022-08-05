using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

public class NativeLibrary
{
    private readonly Dictionary<Type, object> _funcCache = new();
    private readonly Lazy<IntPtr> _libraryHandle;

    public NativeLibrary(string libraryPath)
    {
        LibraryPath = libraryPath;
        _libraryHandle = new Lazy<IntPtr>(() =>
        {
            var handle = PlatformCompat.System.LoadLibrary(libraryPath);
            if (handle == IntPtr.Zero)
            {
                throw new Exception($"Could not load library: {libraryPath}");
            }
            return handle;
        });
    }

    public string LibraryPath { get; }

    public IntPtr LibraryHandle => _libraryHandle.Value;

    public T Load<T>()
    {
        return (T)_funcCache.Get(typeof(T), () => Marshal.GetDelegateForFunctionPointer<T>(LoadFunc<T>())!);
    }

    private IntPtr LoadFunc<T>()
    {
        var symbol = typeof(T).Name.Replace("_delegate", "");
        var ptr = PlatformCompat.System.LoadSymbol(LibraryHandle, symbol);
        if (ptr == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Could not load symbol: {symbol}");
        }
        return ptr;
    }
}