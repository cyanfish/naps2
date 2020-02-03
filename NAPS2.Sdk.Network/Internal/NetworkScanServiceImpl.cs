using System;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images.Storage;
using NAPS2.Platform;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Network.Internal
{
    internal class NetworkScanServiceImpl : NetworkScanService.NetworkScanServiceBase
    {
        private readonly ImageContext imageContext;
        private readonly IRemoteScanController remoteScanController;
        
        public NetworkScanServiceImpl(ImageContext imageContext, IRemoteScanController remoteScanController)
        {
            this.imageContext = imageContext;
            this.remoteScanController = remoteScanController;
        }
        
        public override Task<GetCapabilitiesResponse> GetCapabilities(GetCapabilitiesRequest request, ServerCallContext context)
        {
            var response = new GetCapabilitiesResponse();
            if (PlatformCompat.System.IsWiaDriverSupported)
            {
                response.SupportedDrivers.Add(DriverNames.WIA);
            }
            if (PlatformCompat.System.IsTwainDriverSupported)
            {
                response.SupportedDrivers.Add(DriverNames.TWAIN);
            }
            if (PlatformCompat.System.IsSaneDriverSupported)
            {
                response.SupportedDrivers.Add(DriverNames.SANE);
            }

            return Task.FromResult(response);
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
            var sequencedWriter = new SequencedWriter<ScanResponse>(responseStream);
            try
            {
                var scanEvents = new ScanEvents(
                    () => sequencedWriter.Write(new ScanResponse
                    {
                        PageStart = new PageStartEvent()
                    }),
                    // TODO: Throttle progress. Also whether to send progress at all should be an option somewhere.
                    progress => sequencedWriter.Write(new ScanResponse
                    {
                        Progress = new ProgressEvent
                        {
                            Value = progress
                        }
                    })
                );
                
                var options = request.OptionsXml.FromXml<ScanOptions>();
                options = ValidateOptions(options);
                await remoteScanController.Scan(options, context.CancellationToken, scanEvents, (image, _) =>
                {
                    sequencedWriter.Write(new ScanResponse
                    {
                        Image = SerializedImageHelper.Serialize(imageContext, image, new SerializedImageHelper.SerializeOptions
                        {
                            RequireMemoryStorage = true
                        })
                    });
                });
            }
            catch (Exception e)
            {
                sequencedWriter.Write(new ScanResponse
                {
                    Error = RemotingHelper.ToError(e)
                });
            }
            await sequencedWriter.WaitForCompletion();
        }

        private ScanOptions ValidateOptions(ScanOptions options)
        {
            // No GUI support
            options.UseNativeUI = false;
            options.NoUI = true;
            if (options.TwainOptions.Adapter == TwainAdapter.Legacy)
            {
                options.TwainOptions.Adapter = TwainAdapter.NTwain;
            }
            
            // No OCR (note: this could be changed at some point)
            options.DoOcr = false;
            
            return options;
        }
    }
}
