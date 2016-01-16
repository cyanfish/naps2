using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;

namespace NAPS2.Host
{
    public static class X86HostEntry
    {
        public static void Run(string[] args, X86HostService hostService)
        {
            if (args.All(x => x != X86HostManager.MAGIC_ARG))
            {
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string pipeName = string.Format(X86HostManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                var form = new BackgroundForm();
                hostService.ParentForm = form;

                using (var host = new ServiceHost(hostService))
                {
                    host.AddServiceEndpoint(typeof (IX86HostService),
                        new NetNamedPipeBinding {ReceiveTimeout = TimeSpan.FromHours(24), SendTimeout = TimeSpan.FromHours(24)}, pipeName);
                    host.Open();
                    Console.Write('k');
                    Application.Run(form);
                }
            }
            catch (Exception)
            {
                Console.Write('k');
                throw;
            }
        }
    }
}
