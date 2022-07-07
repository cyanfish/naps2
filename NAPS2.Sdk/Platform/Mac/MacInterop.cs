using System.Runtime.InteropServices;

namespace NAPS2.Platform.Mac;

public static class MacInterop
{
    [DllImport("libSystem.dylib")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libSystem.dylib")]
    public static extern IntPtr dlerror();

    [DllImport("libSystem.dylib")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);
}