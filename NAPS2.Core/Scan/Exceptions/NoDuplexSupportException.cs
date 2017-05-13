using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class NoDuplexSupportException : ScanDriverException
    {
        public NoDuplexSupportException()
            : base(MiscResources.NoDuplexSupport)
        {
        }

        public NoDuplexSupportException(string message)
            : base(message)
        {
        }

        public NoDuplexSupportException(Exception innerException)
            : base(MiscResources.NoDuplexSupport, innerException)
        {
        }

        public NoDuplexSupportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
