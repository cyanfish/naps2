using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.DI;
using NAPS2.Host;
using Ninject;

namespace NAPS2_32
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //if (args.All(x => x != X86HostManager.MAGIC_ARG))
            //{
            //    return;
            //}
                
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string pipeName = string.Format(X86HostManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
            var svc = KernelManager.Kernel.Get<X86HostService>();
            using (var host = new ServiceHost(svc))
            {
                host.AddServiceEndpoint(typeof(IX86HostService), new NetNamedPipeBinding { ReceiveTimeout = TimeSpan.FromHours(24), SendTimeout = TimeSpan.FromHours(24) }, pipeName);
                host.Open();
                Application.Run();
            }
        }
    }
}
