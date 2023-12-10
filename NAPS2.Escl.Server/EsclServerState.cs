namespace NAPS2.Escl.Server;

internal class EsclServerState
{
    public bool IsProcessing { get; set; }

    public Dictionary<string, JobInfo> Jobs { get; } = new();
}