using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class NoDevicesFoundException : ScanDriverException
    {
        public NoDevicesFoundException()
            : base(MiscResources.NoDevicesFound)
        {
        }

        public NoDevicesFoundException(string message)
            : base(message)
        {
        }

        public NoDevicesFoundException(Exception innerException)
            : base(MiscResources.NoDevicesFound, innerException)
        {
        }

        public NoDevicesFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
