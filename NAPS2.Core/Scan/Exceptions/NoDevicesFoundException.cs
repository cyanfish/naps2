using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
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

        protected NoDevicesFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}