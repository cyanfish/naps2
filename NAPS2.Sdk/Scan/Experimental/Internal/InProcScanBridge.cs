using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Util;

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

        public List<ScanDevice> GetDeviceList(ScanOptions options) =>
            remoteScanController.GetDeviceList(options);

        public Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback) =>
            remoteScanController.Scan(options, progress, cancelToken, callback);
    }
}