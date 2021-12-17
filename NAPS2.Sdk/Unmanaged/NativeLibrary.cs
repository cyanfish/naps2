using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NAPS2.Platform.Windows;
using NAPS2.Util;

namespace NAPS2.Unmanaged;

public class NativeLibrary
{
    private const int RTLD_LAZY = 1;
    private const int RTLD_GLOBAL = 8;

    private readonly string _basePath;
    private readonly string _win32Path;
    private readonly string _win64Path;
    private readonly string _linuxPath;
    private readonly string _osxPath;

    private readonly Dictionary<Type, object> _funcCache = new Dictionary<Type, object>();
    private readonly Lazy<IntPtr> _libraryHandle;

    public NativeLibrary(string basePath, string win32Path, string win64Path, string linuxPath, string osxPath)
    {
        _basePath = basePath;
        _win32Path = win32Path;
        _win64Path = win64Path;
        _linuxPath = linuxPath;
        _osxPath = osxPath;
        _libraryHandle = new Lazy<IntPtr>(LoadLibrary);
    }

    public IntPtr LibraryHandle => _libraryHandle.Value;

    public T Load<T>()
    {
        return (T)_funcCache.Get(typeof(T), () => Marshal.GetDelegateForFunctionPointer<T>(LoadFunc<T>()));
    }

    private IntPtr LoadLibrary()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var path = Environment.Is64BitProcess ? _win64Path : _win32Path;
            return Win32.LoadLibrary(Path.Combine(_basePath, path));
        }
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return LinuxInterop.dlopen(Path.Combine(_basePath, _linuxPath), RTLD_LAZY | RTLD_GLOBAL);
        }
        if (Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            return OsxInterop.dlopen(Path.Combine(_basePath, _osxPath), RTLD_LAZY | RTLD_GLOBAL);
        }
        return IntPtr.Zero;
    }

    private IntPtr LoadFunc<T>()
    {
        var symbol = typeof(T).Name.Replace("_delegate", "");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (Environment.Is64BitProcess)
            {
                return Win32.GetProcAddress(LibraryHandle, symbol);
            }
            // Names can be mangled in 32-bit
            for (int i = 0; i < 128; i += 4)
            {
                var address = Win32.GetProcAddress(LibraryHandle, $"_{symbol}@{i}");
                if (address != IntPtr.Zero)
                {
                    return address;
                }
            }
        }
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return LinuxInterop.dlsym(LibraryHandle, symbol);
        }
        if (Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            return OsxInterop.dlsym(LibraryHandle, symbol);
        }
        return IntPtr.Zero;
    }

    private static class LinuxInterop
    {
        [DllImport("libdl.so")]
        public static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so")]
        public static extern IntPtr dlerror();

        [DllImport("libdl.so")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
    }

    private static class OsxInterop
    {
        [DllImport("libSystem.dylib")]
        public static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libSystem.dylib")]
        public static extern IntPtr dlerror();

        [DllImport("libSystem.dylib")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
    }
}