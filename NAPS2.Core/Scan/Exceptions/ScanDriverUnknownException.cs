using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class ScanDriverUnknownException : ScanDriverException
    {
        public ScanDriverUnknownException(Exception innerException)
            : base(MiscResources.UnknownDriverError, innerException)
        {
        }

        public ScanDriverUnknownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
