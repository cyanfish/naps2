using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public abstract class NativeWiaObject : IDisposable
    {
        private bool disposed;
        private IntPtr handle;

        protected NativeWiaObject(IntPtr handle)
        {
            Handle = handle;
        }

        protected NativeWiaObject()
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

        private void EnsureNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                NativeWiaMethods.Release(Handle);
                disposed = true;
            }
        }
    }
}
