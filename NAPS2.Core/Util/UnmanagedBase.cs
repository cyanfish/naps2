using System;
using System.Runtime.InteropServices;

namespace NAPS2.Util
{
    public abstract class UnmanagedBase<T> : IDisposable
    {
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

        protected abstract T GetValue();

        protected abstract void DestroyStructures();

        public static implicit operator IntPtr(UnmanagedBase<T> unmanaged)
        {
            return unmanaged.Pointer;
        }

        #region IDisposable Support

        private bool disposed; // To detect redundant calls

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Pointer != IntPtr.Zero && !disposed)
                    {
                        DestroyStructures();
                        Marshal.FreeHGlobal(Pointer);
                    }
                }
                disposed = true;
            }
        }

        ~UnmanagedBase()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}