using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class NoDuplexSupportException : ScanDriverException
    {
        protected NoDuplexSupportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
    }
}
