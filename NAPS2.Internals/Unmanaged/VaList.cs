#if !NETFRAMEWORK
using System.Runtime.InteropServices;

namespace NAPS2.Unmanaged;

/// <summary>
/// Helper for calling the va_list variant of a C variadic function (e.g. vprintf instead of printf).
/// <para/>
/// P/Invoke can't call variadic functions directly, and declaring fixed-argument signatures for them is unreliable
/// (e.g. on x64 Linux, floating-point arguments are lost unless the caller sets AL).
/// </summary>
public static class VaList
{
    /// <summary>
    /// Calls the provided function with a pointer to a va_list containing the given arguments. The va_list is only
    /// valid for the duration of the call. Arguments must be 8 bytes or smaller.
    /// </summary>
    public static unsafe T Call<T>(Func<IntPtr, T> func, params VaArg[] args)
    {
        ulong* slots = stackalloc ulong[Math.Max(args.Length, 1)];
        for (int i = 0; i < args.Length; i++)
        {
            slots[i] = args[i].Value;
        }

        var arch = RuntimeInformation.ProcessArchitecture;
        if (arch is not (Architecture.X64 or Architecture.Arm64))
        {
            throw new PlatformNotSupportedException();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && arch == Architecture.Arm64)
        {
            return func((IntPtr) slots);
        }
        if (arch == Architecture.X64)
        {
            var vaList = new SysVVaList
            {
                GpOffset = 6 * 8,
                FpOffset = 6 * 8 + 8 * 16,
                OverflowArgArea = slots
            };
            return func((IntPtr) (&vaList));
        }
        var arm64VaList = new Aapcs64VaList
        {
            Stack = slots
        };
        return func((IntPtr) (&arm64VaList));
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SysVVaList
    {
        public uint GpOffset;
        public uint FpOffset;
        public void* OverflowArgArea;
        public void* RegSaveArea;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Aapcs64VaList
    {
        public void* Stack;
        public void* GrTop;
        public void* VrTop;
        public int GrOffs;
        public int VrOffs;
    }
}

/// <summary>
/// An argument to a C variadic function. The implicit conversions apply C's default argument promotions (e.g. short
/// is passed as int and float as double).
/// </summary>
public readonly struct VaArg
{
    private VaArg(ulong value)
    {
        Value = value;
    }

    internal ulong Value { get; }

    public static implicit operator VaArg(int value) => new(unchecked((ulong) value));

    public static implicit operator VaArg(long value) => new(unchecked((ulong) value));

    public static implicit operator VaArg(double value) => new(unchecked((ulong) BitConverter.DoubleToInt64Bits(value)));

    public static implicit operator VaArg(IntPtr value) => new(unchecked((ulong) value.ToInt64()));
}
#endif
