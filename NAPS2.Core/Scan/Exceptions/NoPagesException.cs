using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class NoPagesException : ScanDriverException
    {
        public NoPagesException()
            : base(MiscResources.NoPagesInFeeder)
        {
        }

        public NoPagesException(string message)
            : base(message)
        {
        }

        public NoPagesException(Exception innerException)
            : base(MiscResources.NoPagesInFeeder, innerException)
        {
        }

        public NoPagesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
