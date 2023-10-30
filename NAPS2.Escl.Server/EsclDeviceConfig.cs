namespace NAPS2.Escl.Server;

public class EsclDeviceConfig
{
    public required EsclCapabilities Capabilities { get; init; }

    public required Func<IEsclScanJob> CreateJob { get; init; }

    public int Port { get; set; }
}