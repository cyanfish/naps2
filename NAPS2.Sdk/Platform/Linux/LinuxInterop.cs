using System.Runtime.InteropServices;

namespace NAPS2.Platform.Linux;

public static class LinuxInterop
{
    [DllImport("libc.so.6")]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libc.so.6")]
    public static extern string dlerror();

    [DllImport("libc.so.6")]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libc.so.6")]
    public static extern int setenv(string name, string value, int overwrite);

    [DllImport("libc.so.6")]
    public static extern int readlink(string path, byte[] buffer, int bufferSize);
    
    [DllImport("libc.so.6")]
    public static extern int symlink(string targetPath, string linkPath);
}