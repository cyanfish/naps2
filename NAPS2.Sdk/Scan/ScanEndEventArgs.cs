using System;
using NAPS2.Images;

namespace NAPS2.Scan
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