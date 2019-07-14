using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport;
using NAPS2.Remoting.Worker;
using NAPS2.Util;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";

        private readonly ImageContext imageContext;
        private readonly ITwainWrapper twainWrapper;
        private readonly ScannedImageHelper scannedImageHelper;
        private readonly IWorkerFactory workerFactory;

        //public TwainScanDriver() : base(ErrorOutput.Default, new AutoSaver())
        //{
        //    imageContext = ImageContext.Default;
        //    twainWrapper = new TwainWrapper();
        //    scannedImageHelper = new ScannedImageHelper();
        //}

        public TwainScanDriver(ImageContext imageContext, ITwainWrapper twainWrapper, ScannedImageHelper scannedImageHelper, ErrorOutput errorOutput, AutoSaver autoSaver, IWorkerFactory workerFactory) : base(errorOutput, autoSaver)
        {
            this.imageContext = imageContext;
            this.twainWrapper = twainWrapper;
            this.scannedImageHelper = scannedImageHelper;
            this.workerFactory = workerFactory;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsTwainDriverSupported;
        
        private bool UseWorker(ScanProfile scanProfile) => scanProfile.TwainImpl != TwainImpl.X64 && Environment.Is64BitProcess && PlatformCompat.Runtime.UseWorker;
        
        protected override List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile)
        {
            // Exclude WIA proxy devices since NAPS2 already supports WIA
            return GetFullDeviceList(scanProfile).Where(x => !x.ID.StartsWith("WIA-", StringComparison.InvariantCulture)).ToList();
        }

        private IEnumerable<ScanDevice> GetFullDeviceList(ScanProfile scanProfile)
        {
            throw new NotImplementedException();
        }

        protected override Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            throw new NotImplementedException();
        }
    }
}
