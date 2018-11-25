using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class NoDevicesFoundException : ScanDriverException
    {
        protected NoDevicesFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
