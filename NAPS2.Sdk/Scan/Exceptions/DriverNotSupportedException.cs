using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class DriverNotSupportedException : ScanDriverException
    {
        protected DriverNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
