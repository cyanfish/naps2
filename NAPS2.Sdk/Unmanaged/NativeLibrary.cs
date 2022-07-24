using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

public class NativeLibrary
{
    private readonly Dictionary<Type, object> _funcCache = new();
    private readonly Lazy<IntPtr> _libraryHandle;

    public NativeLibrary(string libraryPath)
    {
        _libraryHandle = new Lazy<IntPtr>(() => PlatformCompat.System.LoadLibrary(libraryPath));
    }

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