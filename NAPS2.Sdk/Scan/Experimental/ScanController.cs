using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Scan.Experimental.Internal;

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

        public List<ScanDevice> GetDeviceList(ScanOptions options)
        {
            options = scanOptionsValidator.ValidateAll(options);
            var bridge = scanBridgeFactory.Create(options);
            return bridge.GetDeviceList(options);
        }

        public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default)
        {
            options = scanOptionsValidator.ValidateAll(options);
            var bridge = scanBridgeFactory.Create(options);
            var sink = new ScannedImageSink();
            int pageNumber = 0;

            void ScanStartCallback() => ScanStart?.Invoke(this, new ScanStartEventArgs());
            void ScanEndCallback(ScannedImageSource source) => ScanEnd?.Invoke(this, new ScanEndEventArgs(source));
            void ScanErrorCallback(Exception ex) => ScanError?.Invoke(this, new ScanErrorEventArgs(ex));
            void PageStartCallback() => PageStart?.Invoke(this, new PageStartEventArgs(++pageNumber));
            void PageProgressCallback(double progress) => PageProgress?.Invoke(this, new PageProgressEventArgs(pageNumber, progress));
            void PageEndCallback(ScannedImage image) => PageEnd?.Invoke(this, new PageEndEventArgs(pageNumber, image));

            ScanStartCallback();
            bridge.Scan(options, cancelToken, new ScanEvents(PageStartCallback, PageProgressCallback), (scannedImage, postProcessingContext) =>
            {
                localPostProcessor.PostProcess(scannedImage, postProcessingContext);
                sink.PutImage(scannedImage);
                PageEndCallback(scannedImage);
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    sink.SetError(t.Exception);
                    ScanErrorCallback(t.Exception);
                }
                else
                {
                    sink.SetCompleted();
                    ScanEndCallback(sink.AsSource());
                }
            });

            return sink.AsSource();
        }

        public event EventHandler<ScanStartEventArgs> ScanStart;

        public event EventHandler<ScanEndEventArgs> ScanEnd;

        public event EventHandler<ScanErrorEventArgs> ScanError;

        public event EventHandler<PageStartEventArgs> PageStart;

        public event EventHandler<PageProgressEventArgs> PageProgress;

        public event EventHandler<PageEndEventArgs> PageEnd;
    }
}
