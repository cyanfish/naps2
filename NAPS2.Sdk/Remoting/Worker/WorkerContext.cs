using Grpc.Core;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
/// </summary>
internal class WorkerContext : IDisposable
{
    /// <summary>
    /// Timeout after attempting to normally stop a worker before it is killed.
    /// </summary>
    private static readonly TimeSpan WorkerStopTimeout = TimeSpan.FromSeconds(60);

    private readonly ILogger _logger;
    private bool _stopped;

    internal WorkerContext(ScanningContext scanningContext, WorkerType workerType, WorkerServiceAdapter service,
        Process process)
    {
        _logger = scanningContext.Logger;
        Type = workerType;
        Service = service;
        Process = process;
    }

    public WorkerType Type { get; }

    internal WorkerServiceAdapter Service { get; }

    public Process Process { get; }

    public async Task Stop()
    {
        if (_stopped) return;
        _stopped = true;

        // Try to cleanly stop the worker
        Task.Run(() =>
        {
            try
            {
                Service.StopWorker();
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.Unavailable)
            {
                // This can happen normally if the system is shutting down (and terminated the worker processes) so we
                // don't log as an error.
                _logger.LogDebug("Could not stop the worker process. It may have crashed.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error stopping worker");
            }
        }).AssertNoAwait();

        // Wait for either the worker process to close or for our timeout
        await Task.WhenAny(Process.WaitForExitAsync(), Task.Delay(WorkerStopTimeout)).ConfigureAwait(false);

        // If the worker process still hasn't closed we kill it now
        if (!Process.HasExited)
        {
            _logger.LogError("Killing unresponsive worker");
            try
            {
                Process.Kill();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error killing unresponsive worker");
            }
        }
    }

    public void Dispose()
    {
        Stop().AssertNoAwait();
    }
}