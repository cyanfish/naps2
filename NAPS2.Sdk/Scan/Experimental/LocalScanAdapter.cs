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
    public class LocalScanAdapter : IScanAdapter
    {
        private readonly IScanDriverController scanDriverController;

        public LocalScanAdapter(IScanDriverController scanDriverController)
        {
            this.scanDriverController = scanDriverController;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options) =>
            scanDriverController.GetDeviceList(options);

        public ScanDevice PromptForDevice(ScanOptions options) =>
            scanDriverController.PromptForDevice(options);

        public Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback) =>
            scanDriverController.Scan(options, progress, cancelToken, callback);
    }
}