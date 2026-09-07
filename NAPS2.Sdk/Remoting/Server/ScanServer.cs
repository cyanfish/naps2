using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
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
        var baseCapabilities = new EsclCapabilities
        {
            MakeAndModel = device.Name,
            Uuid = device.GetUuid(InstanceId),
            IconPng = _defaultIconPng
        };
        return new EsclDeviceConfig
        {
            Port = device.Port,
            TlsPort = device.TlsPort,
            Capabilities = baseCapabilities,
            CapabilitiesProvider = MakeCapabilitiesProvider(device, baseCapabilities),
            CreateJob = settings => new ScanJob(_scanningContext, ScanControllerFactory(), device.Device, settings)
        };
    }

    /// <summary>
    /// Creates a callback that queries the shared device for its actual capabilities (paper sources, resolutions etc.)
    /// so they can be advertised to ESCL clients. The result is queried lazily on the first request and then cached.
    /// If the query fails (e.g. the device is offline or the driver doesn't support querying capabilities), the
    /// default capabilities are used and the query is retried on the next request.
    /// </summary>
    private Func<CancellationToken, Task<EsclCapabilities>> MakeCapabilitiesProvider(
        ScanServerDevice device, EsclCapabilities baseCapabilities)
    {
        EsclCapabilities? cached = null;
        var mutex = new SemaphoreSlim(1, 1);
        return async cancelToken =>
        {
            await mutex.WaitAsync(cancelToken);
            try
            {
                if (cached != null) return cached;
                try
                {
                    var scanCaps = await ScanControllerFactory().GetCaps(device.Device, cancelToken);
                    cached = MergeCaps(baseCapabilities, scanCaps);
                    return cached;
                }
                catch (Exception ex)
                {
                    _scanningContext.Logger.LogError(ex, "Error getting capabilities for shared device");
                    return baseCapabilities;
                }
            }
            finally
            {
                mutex.Release();
            }
        };
    }

    private static EsclCapabilities MergeCaps(EsclCapabilities baseCapabilities, ScanCaps scanCaps)
    {
        var paperSourceCaps = scanCaps.PaperSourceCaps;
        return new EsclCapabilities
        {
            MakeAndModel = baseCapabilities.MakeAndModel,
            Uuid = baseCapabilities.Uuid,
            IconPng = baseCapabilities.IconPng,
            SerialNumber = scanCaps.MetadataCaps?.SerialNumber,
            Manufacturer = scanCaps.MetadataCaps?.Manufacturer,
            PlatenCaps = paperSourceCaps is { SupportsFlatbed: true }
                ? MapInputCaps(scanCaps.FlatbedCaps)
                : null,
            AdfSimplexCaps = paperSourceCaps is { SupportsFeeder: true }
                ? MapInputCaps(scanCaps.FeederCaps)
                : null,
            AdfDuplexCaps = paperSourceCaps is { SupportsDuplex: true }
                ? MapInputCaps(scanCaps.DuplexCaps)
                : null
        };
    }

    private static EsclInputCaps MapInputCaps(PerSourceCaps? caps)
    {
        var inputCaps = new EsclInputCaps();
        if (caps?.PageSizeCaps?.ScanArea is { } scanArea)
        {
            // ESCL width/height values are in 1/300ths of an inch
            inputCaps.MaxWidth = (int) (scanArea.WidthInInches * 300);
            inputCaps.MaxHeight = (int) (scanArea.HeightInInches * 300);
        }
        var colorModes = new List<EsclColorMode>();
        if (caps?.BitDepthCaps is { } bitDepthCaps)
        {
            if (bitDepthCaps.SupportsBlackAndWhite) colorModes.Add(EsclColorMode.BlackAndWhite1);
            if (bitDepthCaps.SupportsGrayscale) colorModes.Add(EsclColorMode.Grayscale8);
            if (bitDepthCaps.SupportsColor) colorModes.Add(EsclColorMode.RGB24);
        }
        // We use CommonValues rather than Values as some drivers report a large or continuous range of resolutions
        // that would be unreasonable to fully enumerate in the capabilities XML.
        var resolutions = caps?.DpiCaps?.CommonValues?.Where(dpi => dpi > 0).ToList();
        if (colorModes.Count > 0 || resolutions is { Count: > 0 })
        {
            inputCaps.SettingProfiles.Add(new EsclSettingProfile
            {
                ColorModes = colorModes,
                DiscreteResolutions =
                    resolutions?.Select(dpi => new DiscreteResolution(dpi, dpi)).ToList() ?? []
            });
        }
        return inputCaps;
    }

    public Task Start() => _esclServer.Start();

    public Task Stop() => _esclServer.Stop();

    public void Dispose() => _esclServer.Dispose();
}