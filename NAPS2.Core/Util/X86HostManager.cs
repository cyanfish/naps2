using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;

namespace NAPS2.Util
{
    public static class X86HostManager
    {
        public const string PIPE_NAME_FORMAT = "net.pipe://localhost/NAPS2_32/{0}/x86host";
        
        private static Process _hostProcess;
        private static string _pipeName;
        private static Lazy<ChannelFactory<IX86HostService>> _channelFactory;

        public static void StartHostProcess()
        {
            if (!Environment.Is64BitProcess)
            {
                return;
            }
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var hostProcessPath = Path.Combine(dir, "NAPS2_32.exe");
            _hostProcess = Process.Start(hostProcessPath);
            if (_hostProcess != null)
            {
                _pipeName = string.Format(PIPE_NAME_FORMAT, _hostProcess.Id);
            }
            else
            {
                Log.Error("Could not start 32-bit host process; terminating.");
                Environment.Exit(1);
                return;
            }

            var job = new Job();
            job.AddProcess(_hostProcess.Handle);

            _channelFactory = new Lazy<ChannelFactory<IX86HostService>>(
                () => new ChannelFactory<IX86HostService>(new NetNamedPipeBinding(), new EndpointAddress(_pipeName)));
        }

        public static IX86HostService Interface
        {
            get
            {
                if (!Environment.Is64BitProcess)
                {
                    return new X86HostService();
                }
                return _channelFactory.Value.CreateChannel();
            }
        }
    }
}
