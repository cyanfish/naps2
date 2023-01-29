using System.Collections.Concurrent;
using GrpcDotNetNamedPipes;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A class to manage the lifecycle of worker processes and hook up the named pipe channels.
/// </summary>
public class WorkerFactory : IWorkerFactory
{
    public const string PIPE_NAME_FORMAT = "NAPS2.Worker.{0}";

    private readonly string _nativeWorkerExePath;
    private readonly string? _winX86WorkerExePath;
    private readonly FileStorageManager? _fileStorageManager;

    private Dictionary<WorkerType, BlockingCollection<WorkerContext>>? _workerQueues;

    public static WorkerFactory CreateDefault(FileStorageManager? fileStorageManager)
    {
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindows())
        {
            return new WorkerFactory(Environment.ProcessPath!, null, fileStorageManager);
        }
#endif
        var exePath = Path.Combine(AssemblyHelper.EntryFolder, "NAPS2.exe");
        var workerExePath = Path.Combine(AssemblyHelper.EntryFolder, "NAPS2.Worker.exe");
        if (!File.Exists(workerExePath))
        {
            workerExePath = Path.Combine(AssemblyHelper.EntryFolder, "lib", "NAPS2.Worker.exe");
        }
        return new WorkerFactory(exePath, workerExePath, fileStorageManager);
    }

    public WorkerFactory(string nativeWorkerExePath, string? winX86WorkerExePath = null,
        FileStorageManager? fileStorageManager = null)
    {
        if (!File.Exists(nativeWorkerExePath))
        {
            throw new InvalidOperationException($"Worker exe does not exist: {nativeWorkerExePath}");
        }
        if (winX86WorkerExePath != null && !File.Exists(winX86WorkerExePath))
        {
            throw new InvalidOperationException($"Worker exe does not exist: {winX86WorkerExePath}");
        }
        _nativeWorkerExePath = nativeWorkerExePath;
        _winX86WorkerExePath = winX86WorkerExePath;
        _fileStorageManager = fileStorageManager;
    }

    private Process StartWorkerProcess(WorkerType workerType)
    {
        var parentId = Process.GetCurrentProcess().Id;
        Process? proc;
        if (workerType == WorkerType.WinX86)
        {
            if (!PlatformCompat.System.SupportsWinX86Worker || _winX86WorkerExePath == null)
            {
                throw new InvalidOperationException("Unexpected worker configuration");
            }
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = _winX86WorkerExePath,
                Arguments = $"{parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }
        else
        {
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = _nativeWorkerExePath,
                Arguments = $"worker {parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }
        if (proc == null)
        {
            throw new Exception("Could not start worker process");
        }

        if (PlatformCompat.System.CanUseWin32)
        {
            try
            {
                var job = new Job();
                job.AddProcess(proc.Handle);
            }
            catch
            {
                proc.Kill();
                throw;
            }
        }

        var readyStr = proc.StandardOutput.ReadLine();
        if (readyStr?.Trim() == "error")
        {
            throw new InvalidOperationException("The worker could not start due to an error. See the worker logs.");
        }

        if (readyStr?.Trim() != "ready")
        {
            throw new InvalidOperationException("Unknown problem starting the worker.");
        }

        return proc;
    }

    private void StartWorkerService(WorkerType workerType)
    {
        Task.Run(() =>
        {
            var proc = StartWorkerProcess(workerType);
            var channel = new NamedPipeChannel(".", string.Format(PIPE_NAME_FORMAT, proc.Id));
            _workerQueues![workerType].Add(new WorkerContext(workerType, new WorkerServiceAdapter(channel), proc));
        });
    }

    private WorkerContext NextWorker(WorkerType workerType)
    {
        StartWorkerService(workerType);
        return _workerQueues![workerType]!.Take();
    }

    public void Init(WorkerFactoryInitOptions? options)
    {
        options ??= new WorkerFactoryInitOptions();
        if (_workerQueues == null)
        {
            _workerQueues = new()
            {
                { WorkerType.Native, new BlockingCollection<WorkerContext>() },
                { WorkerType.WinX86, new BlockingCollection<WorkerContext>() }
            };
            if (options.StartSpareWorkers)
            {
                // We start a "spare" worker so that when we need one, it's immediately ready (and then we'll start another
                // spare for the next request).
                StartWorkerService(WorkerType.Native);
                if (PlatformCompat.System.SupportsWinX86Worker)
                {
                    // On windows as we need 32-bit and 64-bit workers for different things, we will have two spare workers,
                    // which isn't ideal but not a big deal.
                    StartWorkerService(WorkerType.WinX86);
                }
            }
        }
    }

    public WorkerContext Create(WorkerType workerType)
    {
        if (_workerQueues == null)
        {
            throw new InvalidOperationException("WorkerFactory has not been initialized");
        }
        var worker = NextWorker(workerType);
        worker.Service.Init(_fileStorageManager?.FolderPath);
        return worker;
    }

    public void Dispose()
    {
        if (_workerQueues == null) return;
        foreach (var queue in _workerQueues.Values)
        {
            while (queue.TryTake(out var worker))
            {
                worker.Dispose();
            }
        }
    }
}