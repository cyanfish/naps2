using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Sane
{
    public class SaneNotAvailableException : ScanDriverException
    {
        public SaneNotAvailableException() : base(MiscResources.SaneNotAvailable)
        {
        }

        public SaneNotAvailableException(Exception innerException) : base(MiscResources.SaneNotAvailable, innerException)
        {
        }
    }
}
