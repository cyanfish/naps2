using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public abstract class ScanDriverException : Exception
    {
        protected ScanDriverException(string message)
            : base(message)
        {
        }

        protected ScanDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScanDriverException()
        {
        }

        protected ScanDriverException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}