using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class DeviceNotFoundException : ScanDriverException
    {
        public DeviceNotFoundException()
            : base(MiscResources.DeviceNotFound)
        {
        }

        public DeviceNotFoundException(string message)
            : base(message)
        {
        }

        public DeviceNotFoundException(Exception innerException)
            : base(MiscResources.DeviceNotFound, innerException)
        {
        }

        public DeviceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
