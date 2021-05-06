using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Internal
{
    /// <summary>
    /// Represents scanning across a network on a different machine.
    /// </summary>
    internal class NetworkScanBridge : IScanBridge
    {
        private readonly ImageContext _imageContext;

        public NetworkScanBridge() : this(ImageContext.Default)
        {
        }

        public NetworkScanBridge(ImageContext imageContext)
        {
            _imageContext = imageContext;
        }

        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        // TODO: On the network server, make sure to throttle progress events
        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback) => throw new NotImplementedException();
    }
}