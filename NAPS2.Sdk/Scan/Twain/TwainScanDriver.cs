using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        
        private readonly ITwainWrapper twainWrapper;
        private readonly ScannedImageHelper scannedImageHelper;

        public TwainScanDriver() : base(ErrorOutput.Default, new AutoSaver())
        {
            twainWrapper = new TwainWrapper();
            scannedImageHelper = new ScannedImageHelper();
        }

        public TwainScanDriver(ITwainWrapper twainWrapper, ScannedImageHelper scannedImageHelper, ErrorOutput errorOutput, AutoSaver autoSaver) : base(errorOutput, autoSaver)
        {
            this.twainWrapper = twainWrapper;
            this.scannedImageHelper = scannedImageHelper;
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
            var twainImpl = scanProfile?.TwainImpl ?? TwainImpl.Default;
            if (UseWorker(scanProfile))
            {
                using(var worker = WorkerManager.Factory.Create())
                {
                    return worker.Service.TwainGetDeviceList(twainImpl);
                }
            }
            return twainWrapper.GetDeviceList(twainImpl);
        }

        protected override async Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            await Task.Run(async () =>
            {
                if (UseWorker(scanProfile))
                {
                    using (var worker = WorkerManager.Factory.Create())
                    {
                        await worker.Service.TwainScan(scanDevice, scanProfile, scanParams, dialogParent, cancelToken, (img, tempPath) =>
                        {
                            if (!string.IsNullOrEmpty(tempPath)) scannedImageHelper.RunBackgroundOcr(img, scanParams, tempPath);
                            sink.PutImage(img);
                        });
                    }
                }
                else
                {
                    twainWrapper.Scan(dialogParent, scanDevice, scanProfile, scanParams, cancelToken, sink, scannedImageHelper.RunBackgroundOcr);
                }
            });
        }
    }
}
