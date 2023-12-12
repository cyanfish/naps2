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
        _esclServer.Logger = _scanningContext.Logger;
        ScanController = new ScanController(scanningContext);
    }

    internal ScanController ScanController { get; set; }

    /// <summary>
    /// A unique ID that is used to help derive the UUIDs for shared scanners. If you expect to have multiple shared
    /// scanners with the same name/model on the same network it may be useful to set this to a unique value.
    /// </summary>
    public Guid InstanceId { get; set; }

    public void SetDefaultIcon(IMemoryImage icon) =>
        SetDefaultIcon(icon.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

    public void SetDefaultIcon(byte[] iconPng) => _defaultIconPng = iconPng;

    public void RegisterDevice(ScanDevice device, string? displayName = null, int port = 0) =>
        RegisterDevice(new SharedDevice { Device = device, Name = displayName ?? device.Name, Port = port });

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
            Port = device.Port,
            Capabilities = new EsclCapabilities
            {
                MakeAndModel = device.Name,
                Uuid = device.GetUuid(InstanceId),
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
                            DocumentFormats = { ContentTypes.PDF, ContentTypes.JPEG, ContentTypes.PNG }
                        }
                    }
                }
            },
            CreateJob = settings => new ScanJob(_scanningContext, ScanController, device.Device, settings)
        };
    }

    public Task Start() => _esclServer.Start();

    public void Stop() => _esclServer.Stop();

    public void Dispose() => _esclServer.Dispose();
}