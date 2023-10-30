using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobState
{
    public static JobState CreateNewJob(IEsclScanJob job)
    {
        return new JobState
        {
            Id = Guid.NewGuid().ToString("D"),
            Status = JobStatus.Processing,
            LastUpdated = Stopwatch.StartNew(),
            Job = job
        };
    }

    public required string Id { get; init; }

    public required JobStatus Status { get; set; }
    
    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }
}

internal enum JobStatus
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Aborted
}