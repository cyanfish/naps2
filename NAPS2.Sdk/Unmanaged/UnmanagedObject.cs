using System;
using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

/// <summary>
/// Class for implicitly converting structures to unmanaged objects addressed by IntPtr.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnmanagedObject<T> : UnmanagedBase<T>
{
    public UnmanagedObject()
        : this(default)
    {
    }

    public UnmanagedObject(T value)
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