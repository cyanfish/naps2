using System.Runtime.InteropServices;

namespace NAPS2.Scan.Internal.Sane.Native;

internal class SaneOption
{
    private static IEnumerable<string> ParseStringArray(IntPtr arrayPtr)
    {
        for (int i = 0;; i++)
        {
            var ptr = Marshal.ReadIntPtr(arrayPtr + IntPtr.Size * i);
            var str = Marshal.PtrToStringAnsi(ptr);
            if (str == null) break;
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

    internal static SaneOption CreateForTesting(int index, string name, string[] stringList)
    {
        return new SaneOption(index, name, "", "", SaneValueType.String, SaneUnit.None, 0, SaneCapabilities.SoftSelect,
            SaneConstraintType.StringList, stringList.ToList(), null, null);
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

    private SaneOption(int index, string name, string title, string desc, SaneValueType type, SaneUnit unit, int size,
        SaneCapabilities caps, SaneConstraintType constraintType, List<string>? stringList, List<double>? wordList,
        SaneRange? range)
    {
        Index = index;
        Name = name;
        Title = title;
        Desc = desc;
        Type = type;
        Unit = unit;
        Size = size;
        Capabilities = caps;
        ConstraintType = constraintType;
        StringList = stringList;
        WordList = wordList;
        Range = range;
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

    public override string ToString()
    {
        var constraint = StringList != null ? string.Join(",", StringList)
            : WordList != null ? string.Join(",", WordList)
            : Range != null ? $"{Range.Min}-{Range.Max}/{Range.Quant}"
            : "";
        return $"{Index} {Name} {Title} {Desc} {Type} {Unit} {Size} {Capabilities} {ConstraintType} {constraint}";
    }
}