using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    /// <summary>
    /// Represents scanning in the local process.
    /// </summary>
    public class LocalScanBridge : IScanBridge
    {
        private readonly IRemoteScanController remoteScanController;

        public LocalScanBridge()
          : this(new RemoteScanController())
        {
        }

        public LocalScanBridge(IRemoteScanController remoteScanController)
        {
            this.remoteScanController = remoteScanController;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options) =>
            remoteScanController.GetDeviceList(options);

        public ScanDevice PromptForDevice(ScanOptions options) =>
            remoteScanController.PromptForDevice(options);

        public Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback) =>
            remoteScanController.Scan(options, progress, cancelToken, callback);
    }
}