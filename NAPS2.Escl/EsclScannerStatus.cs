namespace NAPS2.Escl;

public class EsclScannerStatus
{
    public EsclScannerState State { get; init; }
    public EsclAdfState AdfState { get; init; }
    public Dictionary<string, EsclJobState> JobStates { get; set; } = new();
}