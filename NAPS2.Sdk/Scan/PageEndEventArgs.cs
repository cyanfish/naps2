namespace NAPS2.Scan;

public class PageEndEventArgs : EventArgs
{
    public PageEndEventArgs(int pageNumber, ProcessedImage image)
    {
        PageNumber = pageNumber;
        Image = image;
    }

    /// <summary>
    /// Gets the page number currently being scanned (starting from 1).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the scanned image.
    /// </summary>
    public ProcessedImage Image { get; set; }
}