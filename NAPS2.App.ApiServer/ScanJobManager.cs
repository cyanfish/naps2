using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.ImageSharp;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.App.ApiServer;

public sealed class ScanJobManager : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly ScanController _scanController;
    private readonly PdfExporter _pdfExporter;
    private readonly ConcurrentDictionary<string, ScanJob> _jobs = new();

    public ScanJobManager()
    {
        _scanningContext = new ScanningContext(new ImageSharpImageContext());
        _scanController = new ScanController(_scanningContext);
        _pdfExporter = new PdfExporter(_scanningContext);
    }

    public ScanJob CreateJob(ScanOptions options)
    {
        var jobId = Guid.NewGuid().ToString("N");
        var job = new ScanJob(jobId, options, _scanController);
        _jobs[jobId] = job;
        job.Start();
        return job;
    }

    public Task<List<ScanDevice>> GetDeviceListAsync(ScanOptions options)
        => _scanController.GetDeviceList(options);

    public Task<ScanCaps> GetCapsAsync(ScanOptions options)
        => _scanController.GetCaps(options);

    public bool TryGetJob(string jobId, out ScanJob job) => _jobs.TryGetValue(jobId, out job!);

    public IEnumerable<ScanJobSummary> GetJobs() => _jobs.Values.Select(job => job.ToSummary());

    public async Task<byte[]> ExportJobPdfAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            throw new KeyNotFoundException($"Scan job \"{jobId}\" not found.");
        }

        if (!job.IsCompleted)
        {
            throw new InvalidOperationException($"Scan job \"{jobId}\" is not complete yet.");
        }

        if (job.Images.Count == 0)
        {
            throw new InvalidOperationException($"Scan job \"{jobId}\" has no scanned images to export.");
        }

        await using var memoryStream = new MemoryStream();
        var success = await _pdfExporter.Export(memoryStream, job.Images);
        if (!success)
        {
            throw new InvalidOperationException("PDF export failed.");
        }

        return memoryStream.ToArray();
    }

    public void Dispose()
    {
        foreach (var job in _jobs.Values)
        {
            job.Dispose();
        }

        _scanningContext.Dispose();
    }
}

public sealed class ScanJob : IDisposable
{
    private readonly ScanController _scanController;
    private readonly CancellationTokenSource _cts = new();

    public ScanJob(string jobId, ScanOptions options, ScanController scanController)
    {
        JobId = jobId;
        Options = options;
        _scanController = scanController;
        StartedAt = DateTime.UtcNow;
        Status = "queued";
    }

    public string JobId { get; }
    public ScanOptions Options { get; }
    public DateTime StartedAt { get; }
    public DateTime? CompletedAt { get; private set; }
    public string Status { get; private set; }
    public string? Message { get; private set; }
    public List<ProcessedImage> Images { get; } = new();
    public Exception? Error { get; private set; }
    public bool IsCompleted => CompletedAt.HasValue;
    public int PagesScanned => Images.Count;

    public void Start()
    {
        Status = "running";
        _ = RunAsync();
    }

    public void Cancel()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    public ScanJobSummary ToSummary()
    {
        return new ScanJobSummary(
            JobId,
            Status,
            Message,
            StartedAt,
            CompletedAt,
            PagesScanned);
    }

    private async Task RunAsync()
    {
        try
        {
            await foreach (var image in _scanController.Scan(Options, _cts.Token))
            {
                Images.Add(image);
            }

            Status = _cts.IsCancellationRequested ? "cancelled" : "completed";
            Message = Status == "completed" ? "Scan finished successfully." : "Scan cancelled.";
        }
        catch (OperationCanceledException)
        {
            Status = "cancelled";
            Message = "Scan was cancelled.";
        }
        catch (Exception ex)
        {
            Status = "failed";
            Error = ex;
            Message = ex.Message;
        }
        finally
        {
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Dispose()
    {
        foreach (var image in Images)
        {
            image.Dispose();
        }

        _cts.Dispose();
    }
}

public sealed record ScanJobSummary(
    string JobId,
    string Status,
    string? Message,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int PagesScanned);
