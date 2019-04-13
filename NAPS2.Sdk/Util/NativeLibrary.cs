using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Util
{
    public class NativeLibrary
    {
        private const int RTLD_LAZY = 1;
        private const int RTLD_GLOBAL = 8;

        private readonly string basePath;
        private readonly string win32Path;
        private readonly string win64Path;
        private readonly string linuxPath;
        private readonly string osxPath;

        private readonly Dictionary<Type, object> funcCache = new Dictionary<Type, object>();
        private readonly Lazy<IntPtr> libraryHandle;

        public NativeLibrary(string basePath, string win32Path, string win64Path, string linuxPath, string osxPath)
        {
            this.basePath = basePath;
            this.win32Path = win32Path;
            this.win64Path = win64Path;
            this.linuxPath = linuxPath;
            this.osxPath = osxPath;
            libraryHandle = new Lazy<IntPtr>(LoadLibrary);
        }

        public IntPtr LibraryHandle => libraryHandle.Value;

        public T Load<T>()
        {
            return (T)funcCache.Get(typeof(T), () => Marshal.GetDelegateForFunctionPointer<T>(LoadFunc<T>()));
        }

        private IntPtr LoadLibrary()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var path = Environment.Is64BitProcess ? win64Path : win32Path;
                return Win32.LoadLibrary(Path.Combine(basePath, path));
            }
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return LinuxInterop.dlopen(Path.Combine(basePath, linuxPath), RTLD_LAZY | RTLD_GLOBAL);
            }
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return OsxInterop.dlopen(Path.Combine(basePath, osxPath), RTLD_LAZY | RTLD_GLOBAL);
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
}
