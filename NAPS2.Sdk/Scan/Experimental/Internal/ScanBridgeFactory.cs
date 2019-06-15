using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class ScanBridgeFactory : IScanBridgeFactory
    {
        private readonly InProcScanBridge inProcScanBridge;
        private readonly WorkerScanBridge workerScanBridge;
        private readonly NetworkScanBridge networkScanBridge;

        public ScanBridgeFactory() : this(new InProcScanBridge(), new WorkerScanBridge(), new NetworkScanBridge())
        {
        }

        public ScanBridgeFactory(InProcScanBridge inProcScanBridge, WorkerScanBridge workerScanBridge, NetworkScanBridge networkScanBridge)
        {
            this.inProcScanBridge = inProcScanBridge;
            this.workerScanBridge = workerScanBridge;
            this.networkScanBridge = networkScanBridge;
        }


        public IScanBridge Create(ScanOptions options)
        {
            if (!string.IsNullOrEmpty(options.NetworkOptions?.Ip))
            {
                // The physical scanner is connected to a different computer, so we connect to a NAPS2 server process over the network
                return networkScanBridge;
            }
            if (options.Driver == Driver.Twain && options.TwainOptions.Dsm != TwainDsm.NewX64 && Environment.Is64BitProcess)
            {
                // 32-bit twain can only be used by a 32-bit process, so we use a separate worker process
                return workerScanBridge;
            }
            return inProcScanBridge;
        }
    }
}