namespace NAPS2.Scan;

public class PageEndEventArgs : EventArgs
{
    public PageEndEventArgs(int pageNumber, ProcessedImage image)
    {
        PageNumber = pageNumber;
        Image = image;
    }

    public int PageNumber { get; }

    public ProcessedImage Image { get; set; }
}