using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

public class ScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<SharedDevice, EsclDeviceConfig> _currentDevices = new();
    private readonly IEsclServer _esclServer;
    private byte[]? _defaultIconPng;

    public ScanServer(ScanningContext scanningContext, IEsclServer esclServer)
    {
        _scanningContext = scanningContext;
        _esclServer = esclServer;
        ScanController = new ScanController(scanningContext);
    }

    internal ScanController ScanController { get; set; }

    public void SetDefaultIcon(IMemoryImage icon) =>
        SetDefaultIcon(icon.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

    public void SetDefaultIcon(byte[] iconPng) => _defaultIconPng = iconPng;

    public void RegisterDevice(ScanDevice device, string? displayName = null) =>
        RegisterDevice(new SharedDevice { Device = device, Name = displayName ?? device.Name });

    public void RegisterDevice(SharedDevice sharedDevice)
    {
        var esclDeviceConfig = MakeEsclDeviceConfig(sharedDevice);
        _currentDevices.Add(sharedDevice, esclDeviceConfig);
        _esclServer.AddDevice(esclDeviceConfig);
    }

    public void UnregisterDevice(ScanDevice device, string? displayName = null) =>
        UnregisterDevice(new SharedDevice { Device = device, Name = displayName ?? device.Name });

    public void UnregisterDevice(SharedDevice sharedDevice)
    {
        var esclDeviceConfig = _currentDevices[sharedDevice];
        _currentDevices.Remove(sharedDevice);
        _esclServer.RemoveDevice(esclDeviceConfig);
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
            CreateJob = settings => new ScanJob(ScanController, device.Device, settings)
        };
    }

    public void Start() => _esclServer.Start();

    public void Stop() => _esclServer.Stop();

    public void Dispose() => _esclServer.Dispose();
}