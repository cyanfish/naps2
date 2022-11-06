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
    private BlockingCollection<WorkerContext>? _workerQueue;

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

    private Process StartWorkerProcess()
    {
        var parentId = Process.GetCurrentProcess().Id;
        Process proc;
        if (PlatformCompat.System.UseSeparateWorkerExe)
        {
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = PlatformCompat.Runtime.ExeRunner ?? WorkerExePath,
                Arguments = PlatformCompat.Runtime.ExeRunner != null ? $"{WorkerExePath} {parentId}" : $"{parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }
        else
        {
#if NET6_0_OR_GREATER
            proc = Process.Start(new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                Arguments = $"worker {parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
#else
            throw new Exception("Unexpected worker configuration");
#endif
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

    private void StartWorkerService()
    {
        Task.Run(() =>
        {
            var proc = StartWorkerProcess();
            var channel = new NamedPipeChannel(".", string.Format(PIPE_NAME_FORMAT, proc.Id));
            _workerQueue!.Add(new WorkerContext(new WorkerServiceAdapter(channel), proc));
        });
    }

    private WorkerContext NextWorker()
    {
        StartWorkerService();
        return _workerQueue!.Take();
    }

    public void Init()
    {
        if (!PlatformCompat.Runtime.UseWorker)
        {
            return;
        }

        if (_workerQueue == null)
        {
            _workerQueue = new BlockingCollection<WorkerContext>();
            StartWorkerService();
        }
    }

    public WorkerContext Create()
    {
        if (_workerQueue == null)
        {
            throw new InvalidOperationException("WorkerFactory has not been initialized");
        }
        var worker = NextWorker();
        worker.Service.Init(_fileStorageManager.FolderPath);
        return worker;
    }
}