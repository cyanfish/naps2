using System.Runtime.InteropServices;

namespace NAPS2.Scan.Internal.Sane.Native;

public class SaneOption
{
    private static IEnumerable<string> ParseStringArray(IntPtr arrayPtr)
    {
        for (int i = 0; ; i++)
        {
            var ptr = Marshal.ReadIntPtr(arrayPtr + IntPtr.Size * i);
            var str = Marshal.PtrToStringAnsi(ptr);
            if (str == null) break;
            Console.WriteLine($"Reading constraint value {str}");
            yield return str;
        }
    }

    private static IEnumerable<double> ParseIntArray(IntPtr arrayPtr)
    {
        int count = Marshal.ReadInt32(arrayPtr);
        for (int i = 0; i < count; i++)
        {
            yield return Marshal.ReadInt32(arrayPtr + (i + 1) * 4);
        }
    }

    private static IEnumerable<double> ParseFixedArray(IntPtr arrayPtr)
    {
        int count = Marshal.ReadInt32(arrayPtr);
        for (int i = 0; i < count; i++)
        {
            yield return SaneFixedPoint.ToFixed(Marshal.ReadInt32(arrayPtr + (i + 1) * 4));
        }
    }

    internal SaneOption(SaneOptionDescriptor descriptor, int index)
    {
        Index = index;
        Name = descriptor.Name;
        Title = descriptor.Title;
        Desc = descriptor.Desc;
        Type = descriptor.Type;
        Unit = descriptor.Unit;
        Size = descriptor.Size;
        Capabilities = descriptor.Capabilities;
        ConstraintType = descriptor.ConstraintType;
        Console.WriteLine($"Creating option {Index} {Name} {Title} {Desc} {Type} {Unit} {Size} {Capabilities} {ConstraintType} {descriptor.Constraint}");
        if (descriptor.ConstraintType == SaneConstraintType.StringList)
        {
            StringList = ParseStringArray(descriptor.Constraint).ToList();
        }
        if (descriptor.ConstraintType == SaneConstraintType.WordList)
        {
            WordList = descriptor.Type == SaneValueType.Fixed
                ? ParseFixedArray(descriptor.Constraint).ToList()
                : ParseIntArray(descriptor.Constraint).ToList();
        }
        if (descriptor.ConstraintType == SaneConstraintType.Range)
        {
            var min = Marshal.ReadInt32(descriptor.Constraint);
            var max = Marshal.ReadInt32(descriptor.Constraint + 4);
            var quant = Marshal.ReadInt32(descriptor.Constraint + 8);
            Range = descriptor.Type == SaneValueType.Fixed
                ? new SaneRange
                {
                    Min = SaneFixedPoint.ToDouble(min),
                    Max = SaneFixedPoint.ToDouble(max),
                    Quant = SaneFixedPoint.ToDouble(quant)
                }
                : new SaneRange
                {
                    Min = min,
                    Max = max,
                    Quant = quant
                };
        }
    }

    public int Index { get; }

    public string? Name { get; }

    public string? Title { get; }

    public string? Desc { get; }

    public SaneValueType Type { get; }

    public SaneUnit Unit { get; }

    public int Size { get; }

    public SaneCapabilities Capabilities { get; }

    public SaneConstraintType ConstraintType { get; }

    public List<string>? StringList { get; }

    public List<double>? WordList { get; }

    public SaneRange? Range { get; }

    public bool IsActive => !Capabilities.HasFlag(SaneCapabilities.Inactive);

    public bool IsSettable => Capabilities.HasFlag(SaneCapabilities.SoftSelect);
}