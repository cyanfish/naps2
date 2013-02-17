using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS.Scan.Driver.Wia;
using NAPS.Scan.Driver.Twain;

namespace NAPS.Scan.Driver
{
    public class DefaultScanDriverFactory : DriverFactory<IScanDriver>, IScanDriverFactory
    {
        public DefaultScanDriverFactory() {
            RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            RegisterDriver(TwainScanDriver.DRIVER_NAME, typeof(TwainScanDriver));
            DefaultDriverName = WiaScanDriver.DRIVER_NAME;
        }
    }
}
