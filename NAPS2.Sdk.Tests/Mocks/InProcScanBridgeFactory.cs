using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests.Mocks
{
    internal class InProcScanBridgeFactory : IScanBridgeFactory
    {
        private readonly InProcScanBridge inProcScanBridge;

        public InProcScanBridgeFactory(InProcScanBridge inProcScanBridge)
        {
            this.inProcScanBridge = inProcScanBridge;
        }
        
        public IScanBridge Create(ScanOptions options)
        {
            return inProcScanBridge;
        }
    }
}