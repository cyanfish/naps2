using System;
using System.Diagnostics;

namespace NAPS2.Logging
{
    public class DebugLogger : ILogger
    {
        public void Error(string message)
        {
            Debug.WriteLine(message);
        }

        public void ErrorException(string message, Exception exception)
        {
            Debug.WriteLine(message);
            Debug.WriteLine(exception.ToString());
        }

        public void FatalException(string message, Exception exception)
        {
            Debug.WriteLine(message);
            Debug.WriteLine(exception.ToString());
        }
    }
}