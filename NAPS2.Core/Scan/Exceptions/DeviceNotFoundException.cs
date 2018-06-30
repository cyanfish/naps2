using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
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

        protected DeviceNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}