using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Internal
{
    /// <summary>
    /// Represents scanning in the local process.
    /// </summary>
    internal class InProcScanBridge : IScanBridge
    {
        private readonly IRemoteScanController _remoteScanController;

        public InProcScanBridge()
          : this(new RemoteScanController())
        {
        }

        public InProcScanBridge(ImageContext imageContext)
            : this(new RemoteScanController(imageContext))
        {
        }

        public InProcScanBridge(IRemoteScanController remoteScanController)
        {
            _remoteScanController = remoteScanController;
        }

        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) =>
            _remoteScanController.GetDeviceList(options);

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback) =>
            _remoteScanController.Scan(options, cancelToken, scanEvents, callback);
    }
}