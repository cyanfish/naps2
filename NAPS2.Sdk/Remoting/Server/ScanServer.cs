using System.Security.Cryptography;
using System.Text;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class ScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<(Driver, string), EsclDeviceConfig> _currentDevices = new();
    private EsclServer? _esclServer;

    public ScanServer(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        ScanController = new ScanController(scanningContext);
    }

    internal ScanController ScanController { get; set; }

    public void RegisterDevice(Driver driver, ScanDevice device, string? name = null)
    {
        var key = (driver, device.ID);
        var esclDeviceConfig = MakeEsclDeviceConfig(driver, device, name ?? device.Name);
        _currentDevices.Add(key, esclDeviceConfig);
        _esclServer?.AddDevice(esclDeviceConfig);
    }

    public void UnregisterDevice(Driver driver, ScanDevice device)
    {
        var key = (driver, device.ID);
        var esclDeviceConfig = _currentDevices[key];
        _currentDevices.Remove(key);
        _esclServer?.RemoveDevice(esclDeviceConfig);
    }

    private EsclDeviceConfig MakeEsclDeviceConfig(Driver driver, ScanDevice device, string name)
    {
        var uniqueHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(device.ID));
        return new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                MakeAndModel = name,
                Uuid = new Guid(uniqueHash.Take(16).ToArray()).ToString("D"),
                // TODO: Ideally we want to get the actual device capabilities
                PlatenCaps = new EsclInputCaps
                {
                    SettingProfiles =
                    {
                        new EsclSettingProfile
                        {
                            ColorModes =
                                { EsclColorMode.RGB24, EsclColorMode.Grayscale8, EsclColorMode.BlackAndWhite1 },
                            XResolutionRange = new ResolutionRange(100, 4800, 300),
                            YResolutionRange = new ResolutionRange(100, 4800, 300),
                            DocumentFormats = { "application/pdf", "image/jpeg" }
                        }
                    }
                }
            },
            CreateJob = () => new ScanJob(ScanController, driver, device)
        };
    }

    public void Start()
    {
        if (_esclServer != null) throw new InvalidOperationException("Already started");
        _esclServer = new EsclServer();
        foreach (var device in _currentDevices.Values)
        {
            _esclServer.AddDevice(device);
        }
        _esclServer.Start();
    }

    public void Stop()
    {
        if (_esclServer == null) throw new InvalidOperationException("Not started");
        _esclServer.Dispose();
        _esclServer = null;
    }

    public void Dispose()
    {
        _esclServer?.Dispose();
        _esclServer = null;
    }
}