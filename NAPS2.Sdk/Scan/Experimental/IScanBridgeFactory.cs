using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public interface IScanBridgeFactory
    {
        IScanBridge Create(ScanOptions options);
    }
}
