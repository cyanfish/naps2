using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Platform;
using NAPS2.Util;

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
            get => _default ?? (_default = new WorkerFactory(ImageContext.Default));
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public const string WORKER_EXE_NAME = "NAPS2.Worker.exe";
        public static readonly string[] SearchDirs =
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

        private (Process, int, string, string) StartWorkerProcess()
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

            var (cert, privateKey) = SslHelper.GenerateRootCertificate();
            WriteEncodedString(proc.StandardInput, cert);
            WriteEncodedString(proc.StandardInput, privateKey);
            var portStr = proc.StandardOutput.ReadLine();
            if (portStr?.Trim() == "error")
            {
                throw new InvalidOperationException("The worker could not start due to an error. See the worker logs.");
            }
            int port = int.Parse(portStr ?? throw new Exception("Could not read worker port"));

            return (proc, port, cert, privateKey);
        }

        private void WriteEncodedString(StreamWriter streamWriter, string value)
        {
            streamWriter.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
        }

        private void StartWorkerService()
        {
            Task.Run(() =>
            {
                var (proc, port, cert, privateKey) = StartWorkerProcess();
                var creds = RemotingHelper.GetClientCreds(cert, privateKey);
                workerQueue.Add(new WorkerContext { Service = new WorkerServiceAdapter(port, creds), Process = proc });
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
