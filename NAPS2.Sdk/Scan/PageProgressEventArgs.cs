namespace NAPS2.Scan;

public class PageProgressEventArgs : EventArgs
{
    public PageProgressEventArgs(int pageNumber, double progress)
    {
        PageNumber = pageNumber;
        Progress = progress;
    }

    /// <summary>
    /// Gets the page number currently being scanned (starting from 1).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the progress of the scan (between 0.0 and 1.0).
    /// </summary>
    public double Progress { get; }
}