using NAPS2.Scan;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A factory interface to spawn NAPS2.Worker.exe instances as needed.
/// </summary>
internal interface IWorkerFactory : IDisposable
{
    void Init(ScanningContext scanningContext, WorkerFactoryInitOptions? options = null);
    WorkerContext Create(ScanningContext scanningContext, WorkerType workerType);
}