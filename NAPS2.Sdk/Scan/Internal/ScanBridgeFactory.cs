using System;
using NAPS2.Images.Storage;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal
{
    internal class ScanBridgeFactory : IScanBridgeFactory
    {
        private readonly InProcScanBridge _inProcScanBridge;
        private readonly WorkerScanBridge _workerScanBridge;
        private readonly NetworkScanBridge _networkScanBridge;

        public ScanBridgeFactory() : this(new InProcScanBridge(), new WorkerScanBridge(), new NetworkScanBridge())
        {
        }

        public ScanBridgeFactory(ImageContext imageContext, IWorkerFactory workerFactory) :
            this(
                new InProcScanBridge(imageContext),
                new WorkerScanBridge(imageContext, workerFactory),
                new NetworkScanBridge(imageContext))
        {
        }

        public ScanBridgeFactory(InProcScanBridge inProcScanBridge, WorkerScanBridge workerScanBridge, NetworkScanBridge networkScanBridge)
        {
            _inProcScanBridge = inProcScanBridge;
            _workerScanBridge = workerScanBridge;
            _networkScanBridge = networkScanBridge;
        }


        public IScanBridge Create(ScanOptions options)
        {
            // TODO: The server process may need to use a worker, so we probably need to add a separate layer
            // TODO: Or, perhaps, with a bit of care we can reuse the IScanBridge interface/factory 
            if (!string.IsNullOrEmpty(options.NetworkOptions?.Ip) && options.NetworkOptions?.Port != null)
            {
                // The physical scanner is connected to a different computer, so we connect to a NAPS2 server process over the network
                return _networkScanBridge;
            }
            if (options.Driver == Driver.Twain && options.TwainOptions.Dsm != TwainDsm.NewX64 && Environment.Is64BitProcess)
            {
                // 32-bit twain can only be used by a 32-bit process, so we use a separate worker process
                return _workerScanBridge;
            }
            return _inProcScanBridge;
        }
    }
}