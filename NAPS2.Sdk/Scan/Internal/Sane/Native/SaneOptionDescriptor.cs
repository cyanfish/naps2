namespace NAPS2.Scan.Internal.Sane.Native;

public struct SaneOptionDescriptor
{
    public string Name;
    public string Title;
    public string Desc;
    public int Type;
    public int Unit;
    public int Size;
    public int Cap;
    public int ConstraintType;
    public IntPtr Constraint;
}