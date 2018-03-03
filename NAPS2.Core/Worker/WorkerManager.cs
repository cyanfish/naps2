using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace NAPS2.Worker
{
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

        private static BlockingCollection<IWorkerService> _workerQueue;

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
            var proc = Process.Start(new ProcessStartInfo {
                FileName = WorkerExePath,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (proc == null)
            {
                throw new Exception("Could not start worker process");
            }

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

            proc.StandardOutput.Read();

            return proc;
        }
        
        private static void StartWorkerService()
        {
            Task.Factory.StartNew(() =>
            {
                var proc = StartWorkerProcess();
                var pipeName = string.Format(PIPE_NAME_FORMAT, proc.Id);
                var channelFactory = new ChannelFactory<IWorkerService>(new NetNamedPipeBinding { SendTimeout = TimeSpan.FromHours(24) },
                    new EndpointAddress(pipeName));
                _workerQueue.Add(channelFactory.CreateChannel());
            });
        }

        public static IWorkerService NextWorker()
        {
            StartWorkerService();
            return _workerQueue.Take();
        }

        public static void Init()
        {
            if (_workerQueue == null)
            {
                _workerQueue = new BlockingCollection<IWorkerService>();
                StartWorkerService();
            }
        }
    }
}
