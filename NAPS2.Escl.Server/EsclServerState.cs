using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace NAPS2.Escl.Server;

internal class EsclServerState
{
    private const int CLEANUP_INTERVAL = 10_000;

    private readonly ILogger _logger;

    private readonly Dictionary<string, JobInfo> _jobDict = new();
    private Timer? _cleanupTimer;

    public EsclServerState(ILogger logger)
    {
        _logger = logger;
    }

    public bool IsProcessing { get; set; }

    public IEnumerable<JobInfo> Jobs => _jobDict.Values.ToList();

    public void AddJob(JobInfo jobInfo)
    {
        lock (this)
        {
            _jobDict.Add(jobInfo.Id, jobInfo);
            _cleanupTimer ??= new Timer(Cleanup, null, CLEANUP_INTERVAL, CLEANUP_INTERVAL);
        }
    }

    public bool TryGetJob(string id,  [MaybeNullWhen(false)] out JobInfo jobInfo)
    {
        lock (this)
        {
            return _jobDict.TryGetValue(id, out jobInfo);
        }
    }

    private void Cleanup(object? state)
    {
        lock (this)
        {
            foreach (var jobInfo in Jobs)
            {
                if (jobInfo.IsEligibleForCleanup)
                {
                    _logger.LogDebug($"Cleaning up job {jobInfo.Id} (state {jobInfo.State}, last updated {jobInfo.LastUpdated.Elapsed.TotalSeconds:F1} seconds ago)");
                    _jobDict.Remove(jobInfo.Id);
                    jobInfo.Job.Dispose();
                    if (_jobDict.Count == 0)
                    {
                        _cleanupTimer?.Dispose();
                        _cleanupTimer = null;
                    }
                }
            }
        }
    }
}