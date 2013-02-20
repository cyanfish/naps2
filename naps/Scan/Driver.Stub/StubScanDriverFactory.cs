using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Driver.Stub
{
    class StubScanDriverFactory : IScanDriverFactory
    {
        public IScanDriver CreateDriver(string driverName)
        {
            return new StubScanDriver();
        }

        public bool HasDriver(string driverName)
        {
            return true;
        }
    }
}
