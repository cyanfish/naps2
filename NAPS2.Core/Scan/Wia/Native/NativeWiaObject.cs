using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Platform;

namespace NAPS2.Scan.Wia.Native
{
    public abstract class NativeWiaObject : IDisposable
    {
        public static WiaVersion DefaultWiaVersion => PlatformCompat.System.IsWia20Supported ? WiaVersion.Wia20 : WiaVersion.Wia10;

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

        protected internal IntPtr Handle
        {
            get
            {
                EnsureNotDisposed();
                return handle;
            }
            protected set => handle = value;
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
