using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public class DeviceException : ScanDriverException
    {
        public DeviceException(string message) : base(message)
        {
        }

        protected DeviceException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected DeviceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        protected DeviceException()
        {
        }
    }
}