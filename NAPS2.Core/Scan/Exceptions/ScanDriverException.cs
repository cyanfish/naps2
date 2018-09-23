using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public abstract class ScanDriverException : Exception
    {
        protected ScanDriverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected ScanDriverException(string message)
            : base(message)
        {
        }

        protected ScanDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
