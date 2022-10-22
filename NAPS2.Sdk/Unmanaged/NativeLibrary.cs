using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

public class NativeLibrary
{
    public static string FindPath(string libraryName, string? baseFolder = null)
    {
        var baseFolders = !string.IsNullOrWhiteSpace(baseFolder)
            ? new[] { baseFolder }
            : new[] { AssemblyHelper.LibFolder, AssemblyHelper.EntryFolder };
        foreach (var actualBaseFolder in baseFolders)
        {
            foreach (var searchPath in PlatformCompat.System.LibrarySearchPaths)
            {
                var path = Path.Combine(actualBaseFolder, searchPath, libraryName);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }
        // TODO: Maybe do this for some platforms?
        // var expectedPath =
        //     Path.Combine(AssemblyHelper.LibFolder, PlatformCompat.System.LibrarySearchPaths[0], libraryName);
        // throw new Exception($"Library does not exist: {expectedPath}");
        // Just the library name so it uses the system search paths
        return libraryName;
    }

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
                var error = PlatformCompat.System.GetLoadError();
                throw new Exception($"Could not load library: \"{libraryPath}\". Error: {error}");
            }
            return handle;
        });
    }

    public string LibraryPath { get; }

    public IntPtr LibraryHandle => _libraryHandle.Value;

    public T Load<T>()
    {
        return (T) _funcCache.Get(typeof(T), () => Marshal.GetDelegateForFunctionPointer<T>(LoadFunc<T>())!);
    }

    private IntPtr LoadFunc<T>()
    {
        var symbol = typeof(T).Name.Replace("_delegate", "");
        var ptr = PlatformCompat.System.LoadSymbol(LibraryHandle, symbol);
        if (ptr == IntPtr.Zero)
        {
            var error = PlatformCompat.System.GetLoadError();
            throw new InvalidOperationException($"Could not load symbol: \"{symbol}\". Error: {error}");
        }
        return ptr;
    }
}