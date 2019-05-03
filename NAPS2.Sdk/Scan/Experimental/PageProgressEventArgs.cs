using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
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
}