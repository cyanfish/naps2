using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Logging
{
    /// <summary>
    /// A default logging implementation that does nothing.
    /// </summary>
    public class NullLogger : ILogger
    {
        public void Error(string message)
        {
        }

        public void ErrorException(string message, Exception exception)
        {
        }

        public void FatalException(string message, Exception exception)
        {
        }
    }
}
