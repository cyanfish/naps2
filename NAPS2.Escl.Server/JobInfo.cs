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
            if (transition == StatusTransition.PageComplete)
            {
                jobInfo.NewImageToTransfer();
            }
            if (transition == StatusTransition.ScanComplete)
            {
                serverState.IsProcessing = false;
                jobInfo.IsScanComplete = true;
            }
        });
        return jobInfo;
    }

    public required string Id { get; init; }

    public required EsclJobState State { get; set; }

    public int ImagesCompleted { get; set; }

    public int ImagesToTransfer { get; set; }

    public required Stopwatch LastUpdated { get; set; }

    public required IEsclScanJob Job { get; set; }

    public SimpleAsyncLock NextDocumentLock { get; } = new();

    public bool NextDocumentReady { get; set; }

    // This is different than EsclJobState.Completed; the ESCL state only transitions to completed once the client has
    // finished querying for documents. IsScanComplete is set to true immediately after the physical scan operation is
    // done.
    public bool IsScanComplete { get; private set; }

    private bool StateIsTerminal => State is EsclJobState.Completed or EsclJobState.Aborted or EsclJobState.Canceled;

    public bool IsEligibleForCleanup => IsScanComplete &&
                                        (StateIsTerminal && LastUpdated.Elapsed > TimeSpan.FromMinutes(1) ||
                                         LastUpdated.Elapsed > TimeSpan.FromMinutes(5));

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

    public void NewImageToTransfer()
    {
        lock (this)
        {
            ImagesToTransfer++;
            LastUpdated = Stopwatch.StartNew();
        }
    }

    public void TransferredDocument()
    {
        lock (this)
        {
            if (Job.ContentType == "application/pdf")
            {
                // Assume all images transferred at once
                ImagesCompleted += ImagesToTransfer;
                ImagesToTransfer = 0;
            }
            else
            {
                ImagesCompleted++;
                ImagesToTransfer--;
            }
            LastUpdated = Stopwatch.StartNew();
        }
    }
}