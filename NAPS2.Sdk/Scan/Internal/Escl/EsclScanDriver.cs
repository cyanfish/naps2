#if ESCL
using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Client;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Escl;

public class EsclScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public EsclScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }
    
    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        // TODO: Run location in a persistent background service
        EsclServiceLocator locator = new EsclServiceLocator();
        var services = await locator.Locate();
        return services.Select(x => new ScanDevice(x.Uuid, x.Name)).ToList();
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        EsclServiceLocator locator = new EsclServiceLocator();
        var services = await locator.Locate();
        var service = services.FirstOrDefault(x => x.Uuid == options.Device!.ID) ??
                      throw new DeviceException(SdkResources.DeviceOffline);
        var client = new EsclClient(service);
        var status = await client.GetStatus();
        var job = await client.CreateScanJob(new EsclScanSettings());
        while (true)
        {
            scanEvents.PageStart();
            byte[] doc;
            try
            {
                // TODO: PDF or jpeg?
                doc = await client.NextDocument(job);
            }
            catch (Exception ex)
            {
                // TODO: Log if not 404 or something (maybe return null from nextdoc)
                break;
            }
            callback(_scanningContext.ImageContext.Load(doc));
        }
    }
}
#endif