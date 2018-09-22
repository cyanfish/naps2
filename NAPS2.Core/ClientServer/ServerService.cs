using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using NAPS2.Platform;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Sane;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;

namespace NAPS2.ClientServer
{
    public class ServerService : IServerService
    {
        private readonly IScanDriverFactory scanDriverFactory;
        private readonly IScanPerformer scanPerformer;

        public ServerService(IScanDriverFactory scanDriverFactory, IScanPerformer scanPerformer)
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

        public List<ScanDevice> GetDeviceList(string driverName)
        {
            var driver = scanDriverFactory.Create(driverName);
            return driver.GetDeviceList();
        }

        public void Scan(ScanProfile scanProfile, ScanParams scanParams)
        {
            var internalParams = new ScanParams
            {
                DetectPatchCodes = scanParams.DetectPatchCodes,
                NoUI = true,
                NoAutoSave = true,
                DoOcr = false
            };
            var callback = OperationContext.Current.GetCallbackChannel<IServerCallback>();
            scanPerformer.PerformScan(scanProfile, internalParams, null, null, image =>
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
                }
            });
        }
    }
}