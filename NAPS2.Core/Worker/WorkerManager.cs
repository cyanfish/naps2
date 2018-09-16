using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using NAPS2.Platform;

namespace NAPS2.Worker
{
    /// <summary>
    /// A class to manage the lifecycle of NAPS2.Worker.exe instances and hook up the WCF channels.
    /// </summary>
    public static class WorkerManager
    {
        public const string PIPE_NAME_FORMAT = "net.pipe://localhost/NAPS2.Worker/{0}";
        public const string WORKER_EXE_NAME = "NAPS2.Worker.exe";
        public static readonly string[] SearchDirs =
        {
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
        };

        private static string _workerExePath;

        private static BlockingCollection<WorkerContext> _workerQueue;

        private static string WorkerExePath
        {
            get
            {
                if (_workerExePath == null)
                {
                    foreach (var dir in SearchDirs)
                    {
                        _workerExePath = Path.Combine(dir, WORKER_EXE_NAME);
                        if (File.Exists(WorkerExePath))
                        {
                            break;
                        }
                    }
                }
                return _workerExePath;
            }
        }

        private static Process StartWorkerProcess()
        {
            var parentId = Process.GetCurrentProcess().Id;
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = PlatformCompat.Runtime.ExeRunner ?? WorkerExePath,
                Arguments = PlatformCompat.Runtime.ExeRunner != null ? $"{WorkerExePath} {parentId}" : $"{parentId}",
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

            proc.StandardOutput.Read();

            return proc;
        }

        private static void StartWorkerService()
        {
            Task.Factory.StartNew(() =>
            {
                var proc = StartWorkerProcess();
                var pipeName = string.Format(PIPE_NAME_FORMAT, proc.Id);
                var callback = new WorkerCallback();
                var instanceContext = new InstanceContext(callback);
                var channelFactory = new DuplexChannelFactory<IWorkerService>(instanceContext,
                    new NetNamedPipeBinding
                    {
                        SendTimeout = TimeSpan.FromHours(24),
                        MaxReceivedMessageSize = int.MaxValue
                    },
                    new EndpointAddress(pipeName));
                var channel = channelFactory.CreateChannel();
                _workerQueue.Add(new WorkerContext { Service = channel, Callback = callback, Process = proc });
            });
        }

        public static WorkerContext NextWorker()
        {
            StartWorkerService();
            return _workerQueue.Take();
        }

        public static void Init()
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
    }
}
