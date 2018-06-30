using NAPS2.Lang.Resources;
using System;

namespace NAPS2.Scan.Exceptions
{
    [Serializable()]
    public class NoPagesException : ScanDriverException
    {
        public NoPagesException()
            : base(MiscResources.NoPagesInFeeder)
        {
        }

        public NoPagesException(string message)
            : base(message)
        {
        }

        public NoPagesException(Exception innerException)
            : base(MiscResources.NoPagesInFeeder, innerException)
        {
        }

        public NoPagesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NoPagesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}