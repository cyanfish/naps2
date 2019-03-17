using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NAPS2.Scan.Exceptions
{
    public class DeviceException : ScanDriverException
    {
        public DeviceException(string message)
            : base(message)
        {
        }

        public DeviceException()
        {
        }
    }
}
