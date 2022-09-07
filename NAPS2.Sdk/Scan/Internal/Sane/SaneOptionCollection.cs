namespace NAPS2.Scan.Internal.Sane;

public class SaneOptionCollection : Dictionary<string, SaneOption>
{
    public void Add(SaneOption option)
    {
        Add(option.Name!, option);
    }
}