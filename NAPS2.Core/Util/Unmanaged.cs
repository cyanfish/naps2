using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Util
{
    /// <summary>
    /// Helper class for converting structures to unmanaged objects addressed by IntPtr.
    /// Use the IDisposable pattern to clean up resources.
    /// </summary>
    public static class Unmanaged
    {
        public static Unmanaged<T> CopyOf<T>(T value) => new Unmanaged<T>(value);

        public static UnmanagedArray<T> CopyOf<T>(T[] value) => new UnmanagedArray<T>(value);
    }

    /// <summary>
    /// Class for implicitly converting structures to unmanaged objects addressed by IntPtr.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Unmanaged<T> : UnmanagedBase<T>
    {
        public Unmanaged()
            : this(default)
        {
        }

        public Unmanaged(T value)
        {
            if (!ReferenceEquals(value, null))
            {
                Size = Marshal.SizeOf(typeof(T));
                Pointer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(value, Pointer, false);
            }
        }

        protected override T GetValue()
        {
            if (Pointer == IntPtr.Zero)
            {
                // T must be a reference type, so this returns null
                return default;
            }
            return (T)Marshal.PtrToStructure(Pointer, typeof(T));
        }

        protected override void DestroyStructures()
        {
            Marshal.DestroyStructure(Pointer, typeof(T));
        }
    }
}
