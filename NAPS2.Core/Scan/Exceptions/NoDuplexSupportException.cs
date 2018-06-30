using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public class NoDuplexSupportException : ScanDriverException
    {
        public NoDuplexSupportException()
            : base(MiscResources.NoDuplexSupport)
        {
        }

        public NoDuplexSupportException(string message)
            : base(message)
        {
        }

        public NoDuplexSupportException(Exception innerException)
            : base(MiscResources.NoDuplexSupport, innerException)
        {
        }

        public NoDuplexSupportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NoDuplexSupportException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}