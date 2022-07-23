using Grpc.Core;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Network.Internal;

internal class NetworkScanServiceImpl : NetworkScanService.NetworkScanServiceBase
{
    private readonly ImageContext _imageContext;
    private readonly IScanBridgeFactory _scanBridgeFactory;
        
    public NetworkScanServiceImpl(ImageContext imageContext, IScanBridgeFactory scanBridgeFactory)
    {
        _imageContext = imageContext;
        _scanBridgeFactory = scanBridgeFactory;
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
            var scanningContext = new ScanningContext(_imageContext);
            var controller = new ScanController(scanningContext);
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
            var bridge = _scanBridgeFactory.Create(options);
            await bridge.Scan(options, context.CancellationToken, scanEvents, (image, _) =>
            {
                sequencedWriter.Write(new ScanResponse
                {
                    Image = ImageSerializer.Serialize(image, new SerializeImageOptions
                    {
                        CrossDevice = true
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
        if (options.TwainOptions.Adapter == TwainAdapter.Legacy)
        {
            options.TwainOptions.Adapter = TwainAdapter.NTwain;
        }

        // Avoid recursive network bridging
        options.NetworkOptions = new NetworkOptions();
            
        return options;
    }
}