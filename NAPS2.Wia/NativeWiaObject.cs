using System;
using System.Runtime.InteropServices;

namespace NAPS2.Wia
{
    public abstract class NativeWiaObject : IDisposable
    {
        public static WiaVersion DefaultWiaVersion
        {
            get
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new InvalidOperationException("Wia is only supported on Windows.");
                }
                // WIA 2.0 for Vista or later.
                return Environment.OSVersion.Version.Major >= 6 ? WiaVersion.Wia20 : WiaVersion.Wia10;
            }
        }

        private bool disposed;
        private IntPtr handle;

        protected NativeWiaObject(WiaVersion version, IntPtr handle)
        {
            if (version == WiaVersion.Default)
            {
                version = DefaultWiaVersion;
            }
            Version = version;
            Handle = handle;
        }

        protected NativeWiaObject(WiaVersion version) : this(version, IntPtr.Zero)
        {
        }

        protected IntPtr Handle
        {
            get
            {
                EnsureNotDisposed();
                return handle;
            }
            set => handle = value;
        }

        public WiaVersion Version { get; }

        private void EnsureNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (Handle != IntPtr.Zero)
                {
                    Marshal.Release(Handle);
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NativeWiaObject()
        {
            Dispose(false);
        }
    }
}
