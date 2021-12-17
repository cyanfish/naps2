namespace NAPS2.Scan;

public class PageProgressEventArgs : EventArgs
{
    public PageProgressEventArgs(int pageNumber, double progress)
    {
        PageNumber = pageNumber;
        Progress = progress;
    }

    public int PageNumber { get; }

    public double Progress { get; }
}