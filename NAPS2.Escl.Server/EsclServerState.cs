namespace NAPS2.Escl.Server;

internal class EsclServerState
{
    public bool IsProcessing { get; set; }

    public Dictionary<string, JobState> Jobs { get; } = new();
}