using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class ScanBridgeFactory : IScanBridgeFactory
    {
        public IScanBridge Create(ScanOptions options)
        {
            if (!string.IsNullOrEmpty(options.NetworkOptions?.Ip))
            {
                // The physical scanner is connected to a different computer, so we connect to a NAPS2 server process over the network
                return new NetworkScanBridge();
            }
            if (options.Driver == Driver.Twain && options.TwainOptions.Dsm != TwainDsm.NewX64 && Environment.Is64BitProcess)
            {
                // 32-bit twain can only be used by a 32-bit process, so we use a separate worker process
                return new WorkerScanBridge();
            }
            return new LocalScanBridge();
        }
    }
}