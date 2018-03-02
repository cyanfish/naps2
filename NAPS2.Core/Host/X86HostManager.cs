using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using NAPS2.Util;

namespace NAPS2.Host
{
    public static class X86HostManager
    {
        public const string PIPE_NAME_FORMAT = "net.pipe://localhost/NAPS2_32/{0}/x86host";
        public const string HOST_EXE_NAME = "NAPS2.exe";
        public const string HOST_ARG = "/32BitHost";

        public static Process StartHostProcess()
        {
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var hostProcessPath = Path.Combine(dir, HOST_EXE_NAME);
            var proc = Process.Start(new ProcessStartInfo {
                FileName = hostProcessPath,
                Arguments = HOST_ARG,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (proc == null)
            {
                throw new Exception("Could not start 32-bit host process");
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

        public static IX86HostService Connect()
        {
            var proc = StartHostProcess();
            var pipeName = string.Format(PIPE_NAME_FORMAT, proc.Id);
            var channelFactory = new ChannelFactory<IX86HostService>(new NetNamedPipeBinding {SendTimeout = TimeSpan.FromHours(24)},
                new EndpointAddress(pipeName));
            return channelFactory.CreateChannel();
        }
    }
}
