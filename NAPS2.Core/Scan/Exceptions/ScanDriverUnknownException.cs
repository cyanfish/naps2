using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public class ScanDriverUnknownException : ScanDriverException
    {
        public ScanDriverUnknownException(Exception innerException) : base(MiscResources.UnknownDriverError, innerException)
        {
        }

        public ScanDriverUnknownException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScanDriverUnknownException(string message) : base(message)
        {
        }

        protected ScanDriverUnknownException()
        {
        }

        protected ScanDriverUnknownException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}