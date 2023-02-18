using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

/// <summary>
/// Class for implicitly converting structures to unmanaged objects addressed by IntPtr.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnmanagedObject<T> : UnmanagedBase<T> where T : notnull
{
    public UnmanagedObject(T value)
    {
        Size = Marshal.SizeOf(typeof(T));
        Pointer = Marshal.AllocHGlobal(Size);
        Marshal.StructureToPtr(value, Pointer, false);
    }

    protected override T GetValue() => (T)Marshal.PtrToStructure(Pointer, typeof(T))!;

    protected override void DestroyStructures() => Marshal.DestroyStructure(Pointer, typeof(T));
}