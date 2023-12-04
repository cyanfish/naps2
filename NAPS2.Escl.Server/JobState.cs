using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobState
{
    public static JobState CreateNewJob(EsclServerState serverState, IEsclScanJob job)
    {
        var state = new JobState
        {
            Id = Guid.NewGuid().ToString("D"),
            Status = JobStatus.Processing,
            LastUpdated = Stopwatch.StartNew(),
            Job = job
        };
        job.RegisterStatusTransitionCallback(transition =>
        {
            if (transition == StatusTransition.CancelJob)
            {
                state.Status = JobStatus.Canceled;
                state.LastUpdated = Stopwatch.StartNew();
            }
            if (transition == StatusTransition.AbortJob)
            {
                state.Status = JobStatus.Aborted;
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

    public required JobStatus Status { get; set; }

    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }
}