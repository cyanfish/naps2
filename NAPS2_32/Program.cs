using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Util;

namespace NAPS2_32
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.All(x => x != X86HostManager.MAGIC_ARG))
            {
                return;
            }
            using (var host = new ServiceHost(typeof (X86HostService)))
            {
                string pipeName = string.Format(X86HostManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                host.AddServiceEndpoint(typeof (IX86HostService), new NetNamedPipeBinding(), pipeName);
                host.Open();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new BackgroundForm(pipeName));
            }
        }
    }
}
