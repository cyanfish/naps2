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

        private static readonly Lazy<ChannelFactory<IX86HostService>> ChannelFactory = new Lazy<ChannelFactory<IX86HostService>>(
                () => new ChannelFactory<IX86HostService>(new NetNamedPipeBinding { SendTimeout = TimeSpan.FromHours(24) }, new EndpointAddress(PipeName)));
        
        private static Process _hostProcess;

        public static string PipeName { get; set; }

        public static void StartHostProcess()
        {
            if (!Environment.Is64BitProcess || _hostProcess != null)
            {
                return;
            }
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var hostProcessPath = Path.Combine(dir, HOST_EXE_NAME);
            _hostProcess = Process.Start(new ProcessStartInfo {
                FileName = hostProcessPath,
                Arguments = HOST_ARG,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (_hostProcess != null)
            {
                PipeName = string.Format(PIPE_NAME_FORMAT, _hostProcess.Id);
            }
            else
            {
                Log.Error("Could not start 32-bit host process");
                return;
            }

            try
            {
                var job = new Job();
                job.AddProcess(_hostProcess.Handle);
            }
            catch
            {
                _hostProcess.Kill();
                throw;
            }

            _hostProcess?.StandardOutput.Read();
        }

        public static IX86HostService Connect()
        {
            if (_hostProcess == null)
            {
                StartHostProcess();
            }
            return ChannelFactory.Value.CreateChannel();
        }
    }
}
