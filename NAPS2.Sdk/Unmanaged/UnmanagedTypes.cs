namespace NAPS2.Unmanaged;

/// <summary>
/// Helper class for converting structures to unmanaged objects addressed by IntPtr.
/// Use the IDisposable pattern to clean up resources.
/// </summary>
public static class UnmanagedTypes
{
    public static UnmanagedObject<T> CopyOf<T>(T value) where T : notnull => new(value);

    public static UnmanagedArray<T> CopyOf<T>(T[] value) where T : notnull => new(value);
}