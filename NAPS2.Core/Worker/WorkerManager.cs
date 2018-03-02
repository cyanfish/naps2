using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace NAPS2.Worker
{
    public static class WorkerManager
    {
        public const string PIPE_NAME_FORMAT = "net.pipe://localhost/NAPS2/{0}/worker";
        public const string WORKER_EXE_NAME = "NAPS2.exe";
        public const string WORKER_EXE_ARG = "/32BitHost";

        public static Process StartHostProcess()
        {
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var workerProcessPath = Path.Combine(dir, WORKER_EXE_NAME);
            var proc = Process.Start(new ProcessStartInfo {
                FileName = workerProcessPath,
                Arguments = WORKER_EXE_ARG,
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

        public static IWorkerService StartWorker()
        {
            var proc = StartHostProcess();
            var pipeName = string.Format(PIPE_NAME_FORMAT, proc.Id);
            var channelFactory = new ChannelFactory<IWorkerService>(new NetNamedPipeBinding {SendTimeout = TimeSpan.FromHours(24)},
                new EndpointAddress(pipeName));
            return channelFactory.CreateChannel();
        }
    }
}
