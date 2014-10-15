using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2
{
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
