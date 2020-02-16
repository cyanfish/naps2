using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GrpcDotNetNamedPipes;
using NAPS2.Images.Storage;
using NAPS2.Platform;

namespace NAPS2.Remoting.Worker
{
    /// <summary>
    /// A class to manage the lifecycle of NAPS2.Worker.exe instances and hook up the WCF channels.
    /// </summary>
    public class WorkerFactory : IWorkerFactory
    {
        private static IWorkerFactory _default;

        public static IWorkerFactory Default
        {
            get => _default ??= new WorkerFactory(ImageContext.Default);
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public const string WORKER_EXE_NAME = "NAPS2.Worker.exe";
        public const string PIPE_NAME_FORMAT = "NAPS2.Worker/{0}";

        public static string[] SearchDirs => new[]
        {
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
        };

        private readonly ImageContext imageContext;

        private string workerExePath;
        private BlockingCollection<WorkerContext> workerQueue;

        public WorkerFactory(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        private string WorkerExePath
        {
            get
            {
                if (workerExePath == null)
                {
                    foreach (var dir in SearchDirs)
                    {
                        workerExePath = Path.Combine(dir, WORKER_EXE_NAME);
                        if (File.Exists(WorkerExePath))
                        {
                            break;
                        }
                    }
                }

                return workerExePath;
            }
        }

        private Process StartWorkerProcess()
        {
            var parentId = Process.GetCurrentProcess().Id;
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = PlatformCompat.Runtime.ExeRunner ?? WorkerExePath,
                Arguments = PlatformCompat.Runtime.ExeRunner != null ? $"{WorkerExePath} {parentId}" : $"{parentId}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
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
                workerQueue.Add(new WorkerContext(new WorkerServiceAdapter(channel), proc));
            });
        }

        private WorkerContext NextWorker()
        {
            StartWorkerService();
            return workerQueue.Take();
        }

        public void Init()
        {
            if (!PlatformCompat.Runtime.UseWorker)
            {
                return;
            }

            if (workerQueue == null)
            {
                workerQueue = new BlockingCollection<WorkerContext>();
                StartWorkerService();
            }
        }

        public WorkerContext Create()
        {
            var rsm = imageContext.FileStorageManager as RecoveryStorageManager;
            var worker = NextWorker();
            worker.Service.Init(rsm?.RecoveryFolderPath);
            return worker;
        }
    }
}