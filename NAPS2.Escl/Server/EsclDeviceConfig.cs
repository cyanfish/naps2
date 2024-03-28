namespace NAPS2.Escl.Server;

public class EsclDeviceConfig
{
    public required EsclCapabilities Capabilities { get; init; }

    public required Func<EsclScanSettings, IEsclScanJob> CreateJob { get; init; }

    public int Port { get; set; }

    public int TlsPort { get; set; }
}