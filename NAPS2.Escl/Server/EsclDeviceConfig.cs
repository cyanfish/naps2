namespace NAPS2.Escl.Server;

public class EsclDeviceConfig
{
    public required EsclCapabilities Capabilities { get; init; }

    /// <summary>
    /// An optional callback that queries the full device capabilities (e.g. paper sources and resolutions from the
    /// physical device). This may be called for each ScannerCapabilities request, so implementations should cache as
    /// needed. If this is null or throws an error, Capabilities is used instead.
    /// </summary>
    public Func<CancellationToken, Task<EsclCapabilities>>? CapabilitiesProvider { get; init; }

    public required Func<EsclScanSettings, IEsclScanJob> CreateJob { get; init; }

    public int Port { get; set; }

    public int TlsPort { get; set; }
}