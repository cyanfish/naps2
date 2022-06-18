using System.Runtime.InteropServices;

namespace NAPS2.Platform.Linux;

public static class LinuxInterop
{
    [DllImport("libdl.so")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so")]
    public static extern IntPtr dlerror();

    [DllImport("libdl.so")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);
}