using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Util
{
    /// <summary>
    /// Base class for implicitly converting structures to unmanaged objects addressed by IntPtr.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UnmanagedBase<T> : IDisposable
    {
        private bool disposed;

        ~UnmanagedBase()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the size of the unmanaged structure in bytes. If the structure is null, this is zero.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Gets a value indicated whether the unmanaged structure is null.
        /// </summary>
        public bool IsNull => Pointer == IntPtr.Zero;

        /// <summary>
        /// Gets a pointer to the unmanaged structure. If the provided value was null, this is IntPtr.Zero.
        /// </summary>
        public IntPtr Pointer { get; protected set; }

        /// <summary>
        /// Gets a managed copy of the unmanaged structure.
        /// </summary>
        public T Value
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("unmanaged");
                }
                return GetValue();
            }
        }

        public void Dispose()
        {
            if (Pointer != IntPtr.Zero && !disposed)
            {
                DestroyStructures();
                Marshal.FreeHGlobal(Pointer);
            }
            disposed = true;
        }

        protected abstract T GetValue();

        protected abstract void DestroyStructures();

        public static implicit operator IntPtr(UnmanagedBase<T> unmanaged) => unmanaged.Pointer;
    }
}