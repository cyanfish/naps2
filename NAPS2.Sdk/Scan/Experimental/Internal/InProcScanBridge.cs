using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental.Internal
{
    /// <summary>
    /// Represents scanning in the local process.
    /// </summary>
    internal class InProcScanBridge : IScanBridge
    {
        private readonly IRemoteScanController remoteScanController;

        public InProcScanBridge()
          : this(new RemoteScanController())
        {
        }

        public InProcScanBridge(IRemoteScanController remoteScanController)
        {
            this.remoteScanController = remoteScanController;
        }

        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) =>
            remoteScanController.GetDeviceList(options);

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback) =>
            remoteScanController.Scan(options, cancelToken, scanEvents, callback);
    }
}