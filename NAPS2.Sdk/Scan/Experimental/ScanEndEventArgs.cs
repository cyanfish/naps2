using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
    public class ScanEndEventArgs : EventArgs
    {
        public ScanEndEventArgs(ScannedImageSource source)
        {
            Source = source;
        }

        public ScannedImageSource Source { get; }
    }
}