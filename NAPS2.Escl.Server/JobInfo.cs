using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobInfo
{
    public static JobInfo CreateNewJob(EsclServerState serverState, IEsclScanJob job)
    {
        var state = new JobInfo
        {
            Id = Guid.NewGuid().ToString("D"),
            State = EsclJobState.Processing,
            LastUpdated = Stopwatch.StartNew(),
            Job = job
        };
        job.RegisterStatusTransitionCallback(transition =>
        {
            if (transition == StatusTransition.CancelJob)
            {
                state.State = EsclJobState.Canceled;
                state.LastUpdated = Stopwatch.StartNew();
            }
            if (transition == StatusTransition.AbortJob)
            {
                state.State = EsclJobState.Aborted;
                state.LastUpdated = Stopwatch.StartNew();
            }
            if (transition == StatusTransition.DeviceIdle)
            {
                serverState.IsProcessing = false;
            }
        });
        return state;
    }

    public required string Id { get; init; }

    public required EsclJobState State { get; set; }

    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }
}