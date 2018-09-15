using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class DeviceException : ScanDriverException
    {
        public DeviceException(string message)
            : base(message)
        {
        }
    }
}
