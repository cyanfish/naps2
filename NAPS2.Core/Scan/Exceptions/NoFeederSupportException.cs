using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public class NoFeederSupportException : ScanDriverException
    {
        public NoFeederSupportException()
            : base(MiscResources.NoFeederSupport)
        {
        }

        public NoFeederSupportException(string message)
            : base(message)
        {
        }

        public NoFeederSupportException(Exception innerException)
            : base(MiscResources.NoFeederSupport, innerException)
        {
        }

        public NoFeederSupportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NoFeederSupportException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}