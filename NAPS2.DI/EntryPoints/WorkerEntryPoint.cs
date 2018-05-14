using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using NAPS2.DI.Modules;
using NAPS2.Util;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI.EntryPoints
{
    public static class WorkerEntryPoint
    {
        public static void Run(string[] args)
        {
            try
            {
#if DEBUG
                // Debugger.Launch();
#endif

                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());
                var workerService = kernel.Get<WorkerService>();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ThreadException += UnhandledException;

                string pipeName = string.Format(WorkerManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                var form = new BackgroundForm();
                workerService.ParentForm = form;

                using (var host = new ServiceHost(workerService))
                {
                    host.AddServiceEndpoint(typeof (IWorkerService),
                        new NetNamedPipeBinding {ReceiveTimeout = TimeSpan.FromHours(24), SendTimeout = TimeSpan.FromHours(24)}, pipeName);
                    host.Open();
                    Console.Write('k');
                    Application.Run(form);
                }
            }
            catch (Exception ex)
            {
                Console.Write('k');
                Log.FatalException("An error occurred that caused the worker application to close.", ex);
                Environment.Exit(1);
            }
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            Log.FatalException("An error occurred that caused the worker to close.", threadExceptionEventArgs.Exception);
        }
    }
}
