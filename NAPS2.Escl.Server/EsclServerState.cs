namespace NAPS2.Escl.Server;

internal class EsclServerState
{
    public Dictionary<string, JobState> Jobs { get; } = new();
}