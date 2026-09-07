using System.Security.Cryptography.X509Certificates;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

/// <summary>
/// Allows scanning devices to be shared across the network. Clients can connect by using Driver.Escl when scanning.
/// </summary>
public class ScanServer : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<ScanServerDevice, EsclDeviceConfig> _currentDevices = new();
    private readonly IEsclServer _esclServer;
    private byte[]? _defaultIconPng;

    public ScanServer(ScanningContext scanningContext, IEsclServer esclServer)
    {
        _scanningContext = scanningContext;
        _esclServer = esclServer;
        _esclServer.Logger = _scanningContext.Logger;
        ScanControllerFactory = () => new ScanController(scanningContext);
    }

    /// <summary>
    /// A unique ID that is used to help derive the UUIDs for shared scanners. If you expect to have multiple shared
    /// scanners with the same name/model on the same network it may be useful to set this to a unique value.
    /// </summary>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// The security policy to use for the ESCL server.
    /// </summary>
    public EsclSecurityPolicy SecurityPolicy
    {
        get => _esclServer.SecurityPolicy;
        set => _esclServer.SecurityPolicy = value;
    }

    /// <summary>
    /// The certificate to be used for TLS connections to the server. If not specified, a self-signed certificate will
    /// be generated when the server starts (unless prevented by the security policy).
    /// </summary>
    public X509Certificate2? Certificate
    {
        get => _esclServer.Certificate;
        set => _esclServer.Certificate = value;
    }

    internal Func<ScanController> ScanControllerFactory { get; set; }

    public void SetDefaultIcon(IMemoryImage icon) =>
        SetDefaultIcon(icon.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

    public void SetDefaultIcon(byte[] iconPng) => _defaultIconPng = iconPng;

    public void RegisterDevice(ScanDevice device, string? displayName = null, int port = 0, int tlsPort = 0) =>
        RegisterDevice(new ScanServerDevice
            { Device = device, Name = displayName ?? device.Name, Port = port, TlsPort = tlsPort });

    private void RegisterDevice(ScanServerDevice sharedDevice)
    {
        var esclDeviceConfig = MakeEsclDeviceConfig(sharedDevice);
        _currentDevices.Add(sharedDevice, esclDeviceConfig);
        _esclServer.AddDevice(esclDeviceConfig);
    }

    public void UnregisterDevice(ScanDevice device, string? displayName = null) =>
        UnregisterDevice(new ScanServerDevice { Device = device, Name = displayName ?? device.Name });

    private void UnregisterDevice(ScanServerDevice sharedDevice)
    {
        var esclDeviceConfig = _currentDevices[sharedDevice];
        _currentDevices.Remove(sharedDevice);
        _esclServer.RemoveDevice(esclDeviceConfig);
    }

    internal (int port, int tlsPort) GetDevicePorts(ScanDevice device, string? displayName = null) =>
        GetDevicePorts(new ScanServerDevice { Device = device, Name = displayName ?? device.Name });

    private (int port, int tlsPort) GetDevicePorts(ScanServerDevice sharedDevice)
    {
        var esclDeviceConfig = _currentDevices[sharedDevice];
        return (esclDeviceConfig.Port, esclDeviceConfig.TlsPort);
    }

    private EsclDeviceConfig MakeEsclDeviceConfig(ScanServerDevice device)
    {
        return new EsclDeviceConfig
        {
            Port = device.Port,
            TlsPort = device.TlsPort,
            Capabilities = new EsclCapabilities
            {
                MakeAndModel = device.Name,
                Uuid = device.GetUuid(InstanceId),
                IconPng = _defaultIconPng,
                // TODO: Ideally we want to get the actual device capabilities (flatbed/feeder, resolution etc.)
            },
            CreateJob = settings => new ScanJob(_scanningContext, ScanControllerFactory(), device.Device, settings)
        };
    }

    public Task Start() => _esclServer.Start();

    public Task Stop() => _esclServer.Stop();

    public void Dispose() => _esclServer.Dispose();
}