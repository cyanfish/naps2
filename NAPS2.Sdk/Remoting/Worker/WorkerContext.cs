using Grpc.Core;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
/// </summary>
public class WorkerContext : IDisposable
{
    /// <summary>
    /// Timeout after attempting to normally stop a worker before it is killed.
    /// </summary>
    private static readonly TimeSpan WorkerStopTimeout = TimeSpan.FromSeconds(60);

    private readonly ILogger _logger;

    internal WorkerContext(ScanningContext scanningContext, WorkerType workerType, WorkerServiceAdapter service, Process process)
    {
        _logger = scanningContext.Logger;
        Type = workerType;
        Service = service;
        Process = process;
    }

    public WorkerType Type { get; }

    internal WorkerServiceAdapter Service { get; }

    public Process Process { get; }

    public void Dispose()
    {
        try
        {
            Service.StopWorker();
            Task.Delay(WorkerStopTimeout).ContinueWith(t =>
            {
                try
                {
                    if (!Process.HasExited)
                    {
                        Process.Kill();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error killing worker");
                }
            });
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Unavailable)
        {
            _logger.LogError("Could not stop the worker process. It may have crashed.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error stopping worker");
        }
    }
}