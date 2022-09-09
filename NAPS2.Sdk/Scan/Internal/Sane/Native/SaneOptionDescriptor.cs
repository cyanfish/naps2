namespace NAPS2.Scan.Internal.Sane.Native;

internal struct SaneOptionDescriptor
{
    public string Name;
    public string Title;
    public string Desc;
    public SaneValueType Type;
    public SaneUnit Unit;
    public int Size;
    public SaneCapabilities Capabilities;
    public SaneConstraintType ConstraintType;
    public IntPtr Constraint;
}