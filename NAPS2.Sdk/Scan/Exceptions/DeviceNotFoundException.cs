using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class DeviceNotFoundException : ScanDriverException
    {
        protected DeviceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
