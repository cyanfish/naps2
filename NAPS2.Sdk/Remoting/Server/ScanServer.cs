using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class ScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<(Driver, string), EsclDeviceConfig> _currentDevices = new();
    private EsclServer? _esclServer;
    private byte[]? _defaultIconPng;

    public ScanServer(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        ScanController = new ScanController(scanningContext);
    }

    internal ScanController ScanController { get; set; }

    public void SetDefaultIcon(IMemoryImage icon) =>
        SetDefaultIcon(icon.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

    public void SetDefaultIcon(byte[] iconPng) => _defaultIconPng = iconPng;

    public void RegisterDevice(SharedDevice device)
    {
        var key = (device.Driver, device.Device.ID);
        var esclDeviceConfig = MakeEsclDeviceConfig(device);
        _currentDevices.Add(key, esclDeviceConfig);
        _esclServer?.AddDevice(esclDeviceConfig);
    }

    public void UnregisterDevice(SharedDevice device)
    {
        var key = (device.Driver, device.Device.ID);
        var esclDeviceConfig = _currentDevices[key];
        _currentDevices.Remove(key);
        _esclServer?.RemoveDevice(esclDeviceConfig);
    }

    private EsclDeviceConfig MakeEsclDeviceConfig(SharedDevice device)
    {
        return new EsclDeviceConfig
        {
            Capabilities = new EsclCapabilities
            {
                MakeAndModel = device.Name,
                Uuid = device.Uuid,
                IconPng = _defaultIconPng,
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
            CreateJob = () => new ScanJob(ScanController, device.Driver, device.Device)
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