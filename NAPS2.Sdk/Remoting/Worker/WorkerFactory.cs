using System.Collections.Concurrent;
using GrpcDotNetNamedPipes;
using NAPS2.Scan;
using NAPS2.Unmanaged;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// A class to manage the lifecycle of worker processes and hook up the named pipe channels.
/// </summary>
public class WorkerFactory : IWorkerFactory
{
    public const string PIPE_NAME_FORMAT = "NAPS2.Worker.{0}";

    private readonly string _nativeWorkerExePath;
    private readonly string? _winX86WorkerExePath;
    private readonly Dictionary<string, string> _environmentVariables;

    private Dictionary<WorkerType, BlockingCollection<WorkerContext>>? _workerQueues;

    public static WorkerFactory CreateDefault()
    {
        var env = new Dictionary<string, string>();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            // The intended way to load sane dependencies (libusb, libjpeg) is by enumerating SaneLibraryDeps and for
            // each, calling dlopen. Then when we load sane it will use those loaded libraries.
            // However, while that works on my arm64 macOS 13, it doesn't on my x64 macOS 10.15. I'm not sure why.
            // But setting DYLD_LIBRARY_PATH on the sane worker process does work.
            // TODO: This means there may be some cases where in-process sane won't work, which could affect SDK users.
            var sanePath = NativeLibrary.FindLibraryPath(PlatformCompat.System.SaneLibraryName);
            if (sanePath.Contains('/'))
            {
                env["DYLD_LIBRARY_PATH"] = Path.GetFullPath(Path.GetDirectoryName(sanePath)!);
            }
        }
        if (!OperatingSystem.IsWindows())
        {
            return new WorkerFactory(Environment.ProcessPath!, null, env);
        }
#endif
        var exePath = Path.Combine(AssemblyHelper.EntryFolder, "NAPS2.exe");
        var workerExePath = Path.Combine(AssemblyHelper.EntryFolder, "NAPS2.Worker.exe");
        if (!File.Exists(workerExePath))
        {
            workerExePath = Path.Combine(AssemblyHelper.EntryFolder, "lib", "NAPS2.Worker.exe");
        }
        return new WorkerFactory(exePath, workerExePath, env);
    }

    public WorkerFactory(string nativeWorkerExePath, string? winX86WorkerExePath = null,
        Dictionary<string, string>? environmentVariables = null)
    {
        _nativeWorkerExePath = nativeWorkerExePath;
        _winX86WorkerExePath = winX86WorkerExePath;
        _environmentVariables = environmentVariables ?? new Dictionary<string, string>();
    }

    private Process StartWorkerProcess(WorkerType workerType)
    {
        var parentId = Process.GetCurrentProcess().Id;
        ProcessStartInfo startInfo;
        if (workerType == WorkerType.WinX86)
        {
            if (!PlatformCompat.System.SupportsWinX86Worker || _winX86WorkerExePath == null)
            {
                throw new InvalidOperationException("Unexpected worker configuration");
            }
            startInfo = new ProcessStartInfo
            {
                FileName = _winX86WorkerExePath,
                Arguments = $"{parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
        }
        else
        {
            startInfo = new ProcessStartInfo
            {
                FileName = _nativeWorkerExePath,
                Arguments = $"worker {parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
        }
        foreach (var name in _environmentVariables.Keys)
        {
            startInfo.EnvironmentVariables[name] = _environmentVariables[name];
        }
        var proc = Process.Start(startInfo);
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

    private void StartWorkerService(ScanningContext scanningContext, WorkerType workerType)
    {
        Task.Run(() =>
        {
            var proc = StartWorkerProcess(workerType);
            var channel = new NamedPipeChannel(".", string.Format(PIPE_NAME_FORMAT, proc.Id));
            _workerQueues![workerType].Add(new WorkerContext(scanningContext, workerType, new WorkerServiceAdapter(channel), proc));
        });
    }

    private WorkerContext NextWorker(ScanningContext scanningContext, WorkerType workerType)
    {
        StartWorkerService(scanningContext, workerType);
        return _workerQueues![workerType]!.Take();
    }

    public void Init(ScanningContext scanningContext, WorkerFactoryInitOptions? options)
    {
        if (!File.Exists(_nativeWorkerExePath))
        {
            throw new InvalidOperationException($"Worker exe does not exist: {_nativeWorkerExePath}");
        }
        if (_winX86WorkerExePath != null && !File.Exists(_winX86WorkerExePath))
        {
            throw new InvalidOperationException($"Worker exe does not exist: {_winX86WorkerExePath}");
        }

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
                StartWorkerService(scanningContext, WorkerType.Native);
                if (PlatformCompat.System.SupportsWinX86Worker)
                {
                    // On windows as we need 32-bit and 64-bit workers for different things, we will have two spare workers,
                    // which isn't ideal but not a big deal.
                    StartWorkerService(scanningContext, WorkerType.WinX86);
                }
            }
        }
    }

    public WorkerContext Create(ScanningContext scanningContext, WorkerType workerType)
    {
        if (_workerQueues == null)
        {
            throw new InvalidOperationException("WorkerFactory has not been initialized");
        }
        var worker = NextWorker(scanningContext, workerType);
        worker.Service.Init(scanningContext.FileStorageManager?.FolderPath);
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