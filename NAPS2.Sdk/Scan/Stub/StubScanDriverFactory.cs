using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Stub
{
    public class StubScanDriverFactory : IScanDriverFactory
    {
        public IScanDriver Create(string driverName) => new StubScanDriver(driverName);
    }
}
