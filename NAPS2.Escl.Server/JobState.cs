using System.Diagnostics;

namespace NAPS2.Escl.Server;

internal class JobState
{
    public static JobState CreateNewJob()
    {
        return new JobState
        {
            Id = Guid.NewGuid().ToString("D"),
            Status = JobStatus.Processing,
            LastUpdated = Stopwatch.StartNew()
        };
    }

    public required string Id { get; init; }

    public required JobStatus Status { get; set; }
    
    public required Stopwatch LastUpdated { get; set; }
}

internal enum JobStatus
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Aborted
}