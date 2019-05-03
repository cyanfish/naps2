using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class PageStartEventArgs : EventArgs
    {
        public PageStartEventArgs(int pageNumber)
        {
            PageNumber = pageNumber;
        }

        public int PageNumber { get; }
    }
}