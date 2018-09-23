using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class ScanDriverUnknownException : ScanDriverException
    {
        protected ScanDriverUnknownException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScanDriverUnknownException(Exception innerException)
            : base(MiscResources.UnknownDriverError, innerException)
        {
        }

        public ScanDriverUnknownException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
