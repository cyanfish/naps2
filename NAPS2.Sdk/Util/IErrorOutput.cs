using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A base interface for objects capable of displaying error output.
    /// </summary>
    public abstract class ErrorOutput
    {
        public static ErrorOutput Default = new StubErrorOutput();

        public abstract void DisplayError(string errorMessage);

        public abstract void DisplayError(string errorMessage, string details);

        public abstract void DisplayError(string errorMessage, Exception exception);
    }
}
