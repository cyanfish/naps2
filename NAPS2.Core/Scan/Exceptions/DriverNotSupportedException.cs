using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class DriverNotSupportedException : ScanDriverException
    {
        public DriverNotSupportedException()
            : base(MiscResources.DriverNotSupported)
        {
        }

        public DriverNotSupportedException(string message)
            : base(message)
        {
        }

        public DriverNotSupportedException(Exception innerException)
            : base(MiscResources.DriverNotSupported, innerException)
        {
        }

        public DriverNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
