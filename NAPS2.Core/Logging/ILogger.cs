using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Logging
{
    /// <summary>
    /// A base interface for logging APIs. Used by the Log class.
    /// </summary>
    public interface ILogger
    {
        void Error(string message);
        void ErrorException(string message, Exception exception);
        void FatalException(string message, Exception exception);
    }
}
