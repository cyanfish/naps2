using System.Collections.Concurrent;
using GrpcDotNetNamedPipes;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A class to manage the lifecycle of NAPS2.Worker.exe instances and hook up the WCF channels.
/// </summary>
public class WorkerFactory : IWorkerFactory
{
    public const string WORKER_EXE_NAME = "NAPS2.Worker.exe";
    public const string PIPE_NAME_FORMAT = "NAPS2.Worker.{0}";

    public static string?[] SearchDirs => new[]
    {
        AssemblyHelper.LibFolder,
        AssemblyHelper.EntryFolder
    };

    private readonly FileStorageManager _fileStorageManager;

    private string? _workerExePath;
    private Dictionary<WorkerType, BlockingCollection<WorkerContext>>? _workerQueues;

    public WorkerFactory(FileStorageManager fileStorageManager)
    {
        _fileStorageManager = fileStorageManager;
    }

    private string WorkerExePath
    {
        get
        {
            if (_workerExePath == null)
            {
                foreach (var dir in SearchDirs.WhereNotNull())
                {
                    _workerExePath = Path.Combine(dir, WORKER_EXE_NAME);
                    if (File.Exists(_workerExePath))
                    {
                        break;
                    }
                }
            }

            return _workerExePath!;
        }
    }

    private Process StartWorkerProcess(WorkerType workerType)
    {
        var parentId = Process.GetCurrentProcess().Id;
        Process? proc;
        if (workerType == WorkerType.WinX86)
        {
            if (!PlatformCompat.System.SupportsWinX86Worker)
            {
                throw new InvalidOperationException("Unexpected worker configuration");
            }
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = WorkerExePath,
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
#if NET6_0_OR_GREATER
                FileName = Environment.ProcessPath,
#else
                FileName = AssemblyHelper.EntryFile,
#endif
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

    public void Init()
    {
        if (_workerQueues == null)
        {
            _workerQueues = new()
            {
                { WorkerType.Native, new BlockingCollection<WorkerContext>() },
                { WorkerType.WinX86, new BlockingCollection<WorkerContext>() }
            };
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

    public WorkerContext Create(WorkerType workerType)
    {
        if (_workerQueues == null)
        {
            throw new InvalidOperationException("WorkerFactory has not been initialized");
        }
        var worker = NextWorker(workerType);
        worker.Service.Init(_fileStorageManager.FolderPath);
        return worker;
    }
}