using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Scan.Experimental.Internal;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public class ScanController : IScanController
    {
        private readonly ILocalPostProcessor localPostProcessor;
        private readonly ScanOptionsValidator scanOptionsValidator;
        private readonly IScanBridgeFactory scanBridgeFactory;

        public ScanController()
          : this(new LocalPostProcessor(), new ScanOptionsValidator(), new ScanBridgeFactory())
        {
        }

        internal ScanController(ILocalPostProcessor localPostProcessor, ScanOptionsValidator scanOptionsValidator, IScanBridgeFactory scanBridgeFactory)
        {
            this.localPostProcessor = localPostProcessor;
            this.scanOptionsValidator = scanOptionsValidator;
            this.scanBridgeFactory = scanBridgeFactory;
        }

        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScannedImageSource Scan(ScanOptions options, ProgressHandler progress = default, CancellationToken cancelToken = default)
        {
            options = scanOptionsValidator.ValidateAll(options);
            var bridge = scanBridgeFactory.Create(options);
            var sink = new ScannedImageSink();
            bridge.Scan(options, progress, cancelToken, (scannedImage, postProcessingContext) =>
            {
                localPostProcessor.PostProcess(scannedImage, postProcessingContext);
                sink.PutImage(scannedImage);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    sink.SetError(t.Exception);
                }
                else
                {
                    sink.SetCompleted();
                }
            });
            return sink.AsSource();
        }
    }
}
