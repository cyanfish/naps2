using System.Runtime.InteropServices;

namespace NAPS2.Platform.Mac;

public static class MacInterop
{
    [DllImport("libSystem.dylib")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libSystem.dylib")]
    public static extern string dlerror();

    [DllImport("libSystem.dylib")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libSystem.dylib")]
    public static extern int setenv(string name, string value, int overwrite);
}