using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Images;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.Scan.Twain
{
    public class TwainScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "twain";
        
        private readonly TwainWrapper twainWrapper;
        private readonly ScannedImageHelper scannedImageHelper;

        public TwainScanDriver()
        {
            twainWrapper = new TwainWrapper();
            scannedImageHelper = new ScannedImageHelper();
        }

        public TwainScanDriver(TwainWrapper twainWrapper, ScannedImageHelper scannedImageHelper)
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

        protected override async Task ScanInternal(ScannedImageSource.Concrete source, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (UseWorker(scanProfile))
                {
                    using (var worker = WorkerManager.Factory.Create())
                    {
                        worker.Callback.ImageCallback += (img, tempPath) =>
                        {
                            if (tempPath != null) scannedImageHelper.RunBackgroundOcr(img, scanParams, tempPath);
                            source.Put(img);
                        };
                        cancelToken.Register(worker.Service.CancelTwainScan);
                        await worker.Service.TwainScan(scanDevice, scanProfile, scanParams, dialogParent);
                    }
                }
                else
                {
                    twainWrapper.Scan(dialogParent, scanDevice, scanProfile, scanParams, cancelToken, source, scannedImageHelper.RunBackgroundOcr);
                }
            }, TaskCreationOptions.LongRunning).Unwrap();
        }
    }
}
