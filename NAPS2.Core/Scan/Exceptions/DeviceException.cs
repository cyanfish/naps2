using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class DeviceException : ScanDriverException
    {
        protected DeviceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DeviceException(string message)
            : base(message)
        {
        }
    }
}
