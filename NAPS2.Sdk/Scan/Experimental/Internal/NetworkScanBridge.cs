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
    /// Represents scanning across a network on a different machine.
    /// </summary>
    internal class NetworkScanBridge : IScanBridge
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback) => throw new NotImplementedException();
    }
}