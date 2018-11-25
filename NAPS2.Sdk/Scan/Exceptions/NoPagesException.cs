using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class NoPagesException : ScanDriverException
    {
        protected NoPagesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
    }
}
