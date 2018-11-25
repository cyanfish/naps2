using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class NoFeederSupportException : ScanDriverException
    {
        protected NoFeederSupportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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
    }
}
