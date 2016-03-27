using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using NAPS2.DI.Modules;
using NAPS2.Host;
using NAPS2.Util;
using Ninject;

namespace NAPS2.DI.EntryPoints
{
    // TODO TODO TODO
    // The NAPS2.Host namespace is all about 64-bit support. The NAPS2_32 process calls X86HostEntry.Run.
    // To enable 64-bit support, simply switch NAPS2 and NAPS2.Console to build in AnyCPU and ensure NAPS2_32.exe is included in distribution.
    // It's not enabled right now because of a few issues - minor, but I don't want to regress the experience for people that don't need the extra memory.
    // Issue list:
    // - Hard to give focus to the TWAIN UI consistently. Maybe leverage the Form.Activated event in NAPS2.exe to call a new method in NAPS2_32.
    // - Relatedly, there's no way to find the TWAIN window from the taskbar. But if the above can work then maybe not needed.
    // - Minor lag (1-2s) when doing the first WCF call. Probably unavoidable.
    // - General stability needs testing/work
    // - Probably something else I forgot. Thorough testing should reveal more issues.
    // TODO TODO TODO
    public static class X86HostEntryPoint
    {
        public static void Run(string[] args)
        {
            try
            {
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());
                var hostService = kernel.Get<X86HostService>();

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
            catch (Exception ex)
            {
                Console.Write('k');
                Log.FatalException("An error occurred that caused the 32-bit host application to close.", ex);
                Environment.Exit(1);
            }
        }
    }
}
