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
        private const string PIPE_BASE_ADDR_FORMAT = "net.pipe://localhost/NAPS2_32/{0}/";

        [STAThread]
        static void Main()
        {
            var pipeBaseAddr = new Uri(string.Format(PIPE_BASE_ADDR_FORMAT, Process.GetCurrentProcess().Id));
            var host = new ServiceHost(typeof(X86HostService), pipeBaseAddr);
            host.Open();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BackgroundForm());
        }
    }
}
