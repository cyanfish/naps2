using System;

namespace NAPS2.Scan.Experimental
{
    public class ScanErrorEventArgs : EventArgs
    {
        public ScanErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}