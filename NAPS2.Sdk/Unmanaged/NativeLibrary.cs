using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

public class NativeLibrary
{
    public static string FindLibraryPath(string libraryName, string? baseFolder = null) =>
        FindPath(libraryName, baseFolder, PlatformCompat.System.LibrarySearchPaths);

    public static string FindExePath(string exeName, string? baseFolder = null) =>
        FindPath(exeName, baseFolder, PlatformCompat.System.ExeSearchPaths);

    private static string FindPath(string libraryName, string? baseFolder, string[] systemSearchPaths)
    {
        var baseFolders = !string.IsNullOrWhiteSpace(baseFolder)
            ? new[] { baseFolder, Path.Combine(baseFolder, "lib") }
            : new[] { AssemblyHelper.LibFolder, AssemblyHelper.EntryFolder };
        foreach (var actualBaseFolder in baseFolders)
        {
            foreach (var searchPath in systemSearchPaths)
            {
                var path = Path.Combine(actualBaseFolder, searchPath, libraryName);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }
        if (baseFolder != null)
        {
            // In tests we definitely expect to find this.
            throw new Exception($"Could not find '{libraryName}' in '{baseFolder}'");
        }
        // Just the library name so it uses the system search paths
        return libraryName;
    }

    private readonly Dictionary<Type, object> _funcCache = new();
    private readonly Lazy<IntPtr> _libraryHandle;

    public NativeLibrary(string libraryPath, string[]? depPaths = null)
    {
        LibraryPath = libraryPath;
        _libraryHandle = new Lazy<IntPtr>(() =>
        {
            if (depPaths != null)
            {
                foreach (var depPath in depPaths)
                {
                    DoLoadLibrary(depPath);
                }
            }
            var handle = DoLoadLibrary(libraryPath);
            return handle;
        });
    }

    private static IntPtr DoLoadLibrary(string path)
    {
        var handle = PlatformCompat.System.LoadLibrary(path);
        if (handle == IntPtr.Zero)
        {
            var error = PlatformCompat.System.GetLoadError();
            throw new Exception($"Could not load library: \"{path}\". Error: {error}");
        }
        return handle;
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