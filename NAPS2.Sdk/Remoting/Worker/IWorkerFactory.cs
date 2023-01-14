namespace NAPS2.Remoting.Worker;

/// <summary>
/// A factory interface to spawn NAPS2.Worker.exe instances as needed.
/// </summary>
public interface IWorkerFactory : IDisposable
{
    void Init();
    WorkerContext Create(WorkerType workerType);
}