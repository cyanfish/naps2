using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS.Scan.Driver
{
    public class ScanDriverException : Exception
    {
        private const string DEFAULT_MESSAGE = "An error occured with the scanning driver.";

        public ScanDriverException()
            : base(DEFAULT_MESSAGE)
        {
        }

        public ScanDriverException(string message)
            : base(message)
        {
        }

        public ScanDriverException(Exception innerException)
            : base(DEFAULT_MESSAGE, innerException)
        {
        }

        public ScanDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
