using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental.Internal
{
    internal interface IScanDriverFactory
    {
        IScanDriver Create(ScanOptions options);
    }
}
