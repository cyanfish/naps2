using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Scan;
using NAPS2.Scan.Experimental;
using NAPS2.Scan.Experimental.Internal;

namespace NAPS2.Sdk.Tests.Mocks
{
    internal class StubScanBridge : IScanBridge
    {
        public List<ScanDevice> MockDevices { get; set; } = new List<ScanDevice>();

        public List<ScannedImage> MockOutput { get; set; } = new List<ScannedImage>();
        
        public List<ScanDevice> GetDeviceList(ScanOptions options) => MockDevices;

        public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback)
        {
            await Task.Run(() =>
            {
                foreach (var img in MockOutput)
                {
                    callback(img, new PostProcessingContext());
                }
            });
        }
    }
}
