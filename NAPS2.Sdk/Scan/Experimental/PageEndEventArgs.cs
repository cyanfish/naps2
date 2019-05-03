using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
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
}