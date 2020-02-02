using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Network
{
    internal class ScanService : NetworkScanService.NetworkScanServiceBase
    {
        public override Task<GetCapabilitiesResponse> GetCapabilities(GetCapabilitiesRequest request, ServerCallContext context)
        {
            return base.GetCapabilities(request, context);
        }

        public override async Task<GetDeviceListResponse> GetDeviceList(GetDeviceListRequest request, ServerCallContext context)
        {
            try
            {
                var controller = new ScanController();
                var options = request.OptionsXml.FromXml<ScanOptions>();
                var devices = await controller.GetDeviceList(options);
                return new GetDeviceListResponse
                {
                    DeviceListXml = devices.ToXml()
                };
            }
            catch (Exception e)
            {
                return new GetDeviceListResponse
                {
                    Error = RemotingHelper.ToError(e)
                };
            }
        }

        public override async Task Scan(ScanRequest request, IServerStreamWriter<ScanResponse> responseStream, ServerCallContext context)
        {
            try
            {
                // TODO
            }
            catch (Exception e)
            {
                await responseStream.WriteAsync(new ScanResponse
                {
                    Error = RemotingHelper.ToError(e)
                });
            }
        }

        // private readonly IRemoteScanController remoteScanController;
        //
        // private CancellationTokenSource scanCts = new CancellationTokenSource();
        //
        // public ScanService() : this(new RemoteScanController())
        // {
        // }
        //
        // internal ScanService(IRemoteScanController remoteScanController)
        // {
        //     this.remoteScanController = remoteScanController;
        // }

        public List<string> GetSupportedDriverNames()
        {
            throw new NotImplementedException();
//            var driverNames = new List<string>();
//            if (PlatformCompat.System.IsWiaDriverSupported)
//            {
//                driverNames.Add(WiaScanDriver.DRIVER_NAME);
//            }
//
//            if (PlatformCompat.System.IsTwainDriverSupported)
//            {
//                driverNames.Add(TwainScanDriver.DRIVER_NAME);
//            }
//
//            if (PlatformCompat.System.IsSaneDriverSupported)
//            {
//                driverNames.Add(SaneScanDriver.DRIVER_NAME);
//            }
//
//            return driverNames;
        }

        public List<ScanDevice> GetDeviceList(ScanProfile scanProfile)
        {
            throw new NotImplementedException();
//            if (scanProfile.DriverName == ProxiedScanDriver.DRIVER_NAME)
//            {
//                scanProfile.DriverName = scanProfile.ProxyDriverName;
//            }
//            var driver = scanDriverFactory.Create(scanProfile.DriverName);
//            return driver.GetDeviceList(scanProfile);
        }

        public async Task<int> Scan(ScanProfile scanProfile, ScanParams scanParams)
        {
            throw new NotImplementedException();
//            if (scanProfile.DriverName == ProxiedScanDriver.DRIVER_NAME)
//            {
//                scanProfile.DriverName = scanProfile.ProxyDriverName;
//            }
//            if (scanProfile.TwainImpl == TwainImpl.Legacy)
//            {
//                scanProfile.TwainImpl = TwainImpl.OldDsm;
//            }
//            scanProfile.UseNativeUI = false;
//
//            // TODO: Turn PropagateErrors on?
//            var internalParams = new ScanParams
//            {
//                DetectPatchCodes = scanParams.DetectPatchCodes,
//                NoUI = true,
//                NoAutoSave = true,
//                DoOcr = false,
//                SkipPostProcessing = true
//            };
//
//            var callback = OperationContext.Current.GetCallbackChannel<IScanCallback>();
//
//            int pages = 0;
//            var source = await scanPerformer.PerformScan(scanProfile, internalParams, cancelToken: scanCts.Token);
//            await source.ForEach(image =>
//            {
//                // TODO: Should stream this
//                // TODO: Also should think about avoiding the intermediate filesystem
//                using (image)
//                {
//                    // TODO
//                    //var indexImage = image.RecoveryIndexImage;
//                    //var imageBytes = File.ReadAllBytes(image.RecoveryFilePath);
//                    //var sanitizedIndexImage = new RecoveryIndexImage
//                    //{
//                    //    FileName = Path.GetExtension(indexImage.FileName),
//                    //    TransformList = indexImage.TransformList,
//                    //    BitDepth = indexImage.BitDepth,
//                    //    HighQuality = indexImage.HighQuality
//                    //};
//                    //callback.ImageReceived(imageBytes, sanitizedIndexImage);
//                    //pages++;
//                }
//            });
//            return pages;
        }

        public void CancelScan()
        {
            // scanCts.Cancel();
        }
    }
}
