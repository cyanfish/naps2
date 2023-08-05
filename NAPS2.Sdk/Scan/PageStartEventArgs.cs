namespace NAPS2.Scan;

public class PageStartEventArgs : EventArgs
{
    public PageStartEventArgs(int pageNumber)
    {
        PageNumber = pageNumber;
    }

    /// <summary>
    /// Gets the page number currently being scanned (starting from 1).
    /// </summary>
    public int PageNumber { get; }
}