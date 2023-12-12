using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobInfo
{
    public static JobInfo CreateNewJob(EsclServerState serverState, IEsclScanJob job)
    {
        var jobInfo = new JobInfo
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
                jobInfo.TransitionState(EsclJobState.Processing, EsclJobState.Canceled);
            }
            if (transition == StatusTransition.AbortJob)
            {
                jobInfo.TransitionState(EsclJobState.Processing, EsclJobState.Aborted);
            }
            if (transition == StatusTransition.DeviceIdle)
            {
                serverState.IsProcessing = false;
            }
        });
        return jobInfo;
    }

    public required string Id { get; init; }

    public required EsclJobState State { get; set; }

    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }

    public void TransitionState(EsclJobState precondition, EsclJobState newState)
    {
        lock (this)
        {
            if (State == precondition)
            {
                State = newState;
                LastUpdated = Stopwatch.StartNew();
            }
        }
    }
}