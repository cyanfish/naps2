using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan
{
    public interface IScanDriverFactory
    {
        IScanDriver Create(string driverName);
    }
}
