using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public class RemoteScanController : IRemoteScanController
    {
        private readonly IScanDriverFactory scanDriverFactory;
        private readonly IRemotePostProcessor remotePostProcessor;

        public RemoteScanController()
          : this(new ScanDriverFactory(), new RemotePostProcessor())
        {
        }

        public RemoteScanController(IScanDriverFactory scanDriverFactory, IRemotePostProcessor remotePostProcessor)
        {
            this.scanDriverFactory = scanDriverFactory;
            this.remotePostProcessor = remotePostProcessor;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options) =>
            scanDriverFactory.Create(options).GetDeviceList(options);

        public ScanDevice PromptForDevice(ScanOptions options) =>
            scanDriverFactory.Create(options).PromptForDevice(options);

        public async Task Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<ScannedImage, PostProcessingContext> callback)
        {
            var driver = scanDriverFactory.Create(options);
            await driver.Scan(options, progress, cancelToken, image =>
            {
                var (scannedImage, postProcessingContext) = remotePostProcessor.PostProcess(image);
                callback(scannedImage, postProcessingContext);
            });
        }
    }
}
