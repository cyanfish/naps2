using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Exceptions
{
    public abstract class ScanDriverException : Exception
    {
        protected ScanDriverException(string message)
            : base(message)
        {
        }

        protected ScanDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
