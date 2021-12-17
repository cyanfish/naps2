namespace NAPS2.Scan;

public class PageStartEventArgs : EventArgs
{
    public PageStartEventArgs(int pageNumber)
    {
        PageNumber = pageNumber;
    }

    public int PageNumber { get; }
}