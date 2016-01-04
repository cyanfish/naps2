using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using NAPS2.Scan.Twain;

namespace NAPS2.Util
{
    public static class X86HostManager
    {
        public const string PIPE_NAME_FORMAT = "net.pipe://localhost/NAPS2_32/{0}/x86host";
        /// <summary>
        /// Use a magic argument when starting NAPS2_32.exe to avoid the user accidentally starting it and having it never stop
        /// </summary>
        public const string MAGIC_ARG = "{DE401010-6942-41D5-9BB0-B1B99A32C4BE}";
        
        private static Process _hostProcess;

        public static string PipeName { get; set; }

        public static void StartHostProcess()
        {
            if (!Environment.Is64BitProcess)
            {
                return;
            }
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var hostProcessPath = Path.Combine(dir, "NAPS2_32.exe");
            _hostProcess = Process.Start(hostProcessPath, MAGIC_ARG);
            if (_hostProcess != null)
            {
                PipeName = string.Format(PIPE_NAME_FORMAT, _hostProcess.Id);
            }
            else
            {
                Log.Error("Could not start 32-bit host process; terminating.");
                Environment.Exit(1);
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
        }
    }
}
