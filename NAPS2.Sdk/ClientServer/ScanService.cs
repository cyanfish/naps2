using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Sane;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;

namespace NAPS2.ClientServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ScanService : IScanService
    {
        private readonly IScanDriverFactory scanDriverFactory;
        private readonly IScanPerformer scanPerformer;

        private CancellationTokenSource scanCts = new CancellationTokenSource();

        public ScanService(IScanDriverFactory scanDriverFactory, IScanPerformer scanPerformer)
        {
            this.scanDriverFactory = scanDriverFactory;
            this.scanPerformer = scanPerformer;
        }

        public List<string> GetSupportedDriverNames()
        {
            var driverNames = new List<string>();
            if (PlatformCompat.System.IsWiaDriverSupported)
            {
                driverNames.Add(WiaScanDriver.DRIVER_NAME);
            }

            if (PlatformCompat.System.IsTwainDriverSupported)
            {
                driverNames.Add(TwainScanDriver.DRIVER_NAME);
            }

            if (PlatformCompat.System.IsSaneDriverSupported)
            {
                driverNames.Add(SaneScanDriver.DRIVER_NAME);
            }

            return driverNames;
        }

        public List<ScanDevice> GetDeviceList(ScanProfile scanProfile)
        {
            if (scanProfile.DriverName == ProxiedScanDriver.DRIVER_NAME)
            {
                scanProfile.DriverName = scanProfile.ProxyDriverName;
            }
            var driver = scanDriverFactory.Create(scanProfile.DriverName);
            driver.ScanProfile = scanProfile;
            return driver.GetDeviceList();
        }

        public async Task<int> Scan(ScanProfile scanProfile, ScanParams scanParams)
        {
            if (scanProfile.DriverName == ProxiedScanDriver.DRIVER_NAME)
            {
                scanProfile.DriverName = scanProfile.ProxyDriverName;
            }
            if (scanProfile.TwainImpl == TwainImpl.Legacy)
            {
                scanProfile.TwainImpl = TwainImpl.OldDsm;
            }
            scanProfile.UseNativeUI = false;

            var internalParams = new ScanParams
            {
                DetectPatchCodes = scanParams.DetectPatchCodes,
                NoUI = true,
                NoAutoSave = true,
                DoOcr = false,
                NoThumbnails = true,
                SkipPostProcessing = true
            };

            var callback = OperationContext.Current.GetCallbackChannel<IScanCallback>();

            int pages = 0;
            await scanPerformer.PerformScan(scanProfile, internalParams, null, null, image =>
            {
                // TODO: Should stream this
                // TODO: Also should think about avoiding the intermediate filesystem
                using (image)
                {
                    var indexImage = image.RecoveryIndexImage;
                    var imageBytes = File.ReadAllBytes(image.RecoveryFilePath);
                    var sanitizedIndexImage = new RecoveryIndexImage
                    {
                        FileName = Path.GetExtension(indexImage.FileName),
                        TransformList = indexImage.TransformList,
                        BitDepth = indexImage.BitDepth,
                        HighQuality = indexImage.HighQuality
                    };
                    callback.ImageReceived(imageBytes, sanitizedIndexImage);
                    pages++;
                }
            }, scanCts.Token);
            return pages;
        }

        public void CancelScan()
        {
            scanCts.Cancel();
        }
    }
}
