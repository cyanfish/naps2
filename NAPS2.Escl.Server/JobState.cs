using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobState
{
    public static JobState CreateNewJob(IEsclScanJob job)
    {
        var state = new JobState
        {
            Id = Guid.NewGuid().ToString("D"),
            Status = JobStatus.Processing,
            LastUpdated = Stopwatch.StartNew(),
            Job = job
        };
        job.RegisterStatusChangeCallback(status =>
        {
            state.Status = status;
            state.LastUpdated = Stopwatch.StartNew();
        });
        return state;
    }

    public required string Id { get; init; }

    public required JobStatus Status { get; set; }
    
    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }
}