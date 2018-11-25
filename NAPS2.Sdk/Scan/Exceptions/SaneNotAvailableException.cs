using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    [Serializable]
    public class SaneNotAvailableException : ScanDriverException
    {
        private const string PACKAGES = "\nsane\nsane-utils";

        protected SaneNotAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public SaneNotAvailableException() : base(MiscResources.SaneNotAvailable + PACKAGES)
        {
        }

        public SaneNotAvailableException(Exception innerException) : base(MiscResources.SaneNotAvailable + PACKAGES, innerException)
        {
        }
    }
}
