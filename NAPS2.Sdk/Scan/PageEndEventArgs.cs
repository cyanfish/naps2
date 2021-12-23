namespace NAPS2.Scan;

public class PageEndEventArgs : EventArgs
{
    public PageEndEventArgs(int pageNumber, RenderableImage image)
    {
        PageNumber = pageNumber;
        Image = image;
    }

    public int PageNumber { get; }

    public RenderableImage Image { get; set; }
}