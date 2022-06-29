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

    public WorkerContext(WorkerServiceAdapter service, Process process)
    {
        Service = service;
        Process = process;
    }
        
    public WorkerServiceAdapter Service { get; }

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
                    Log.ErrorException("Error killing worker", e);
                }
            });
        }
        catch (Exception e)
        {
            Log.ErrorException("Error stopping worker", e);
        }
    }
}