using System;
using NAPS2.Images;

namespace NAPS2.Scan;

public class PageEndEventArgs : EventArgs
{
    public PageEndEventArgs(int pageNumber, ScannedImage image)
    {
        PageNumber = pageNumber;
        Image = image;
    }

    public int PageNumber { get; }

    public ScannedImage Image { get; set; }
}