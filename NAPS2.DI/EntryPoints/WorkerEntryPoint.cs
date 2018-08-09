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
    /// <summary>
    /// The entry point for NAPS2.Worker.exe, an off-process worker.
    ///
    /// Unlike NAPS2.exe which is restricted by driver use, NAPS2.Worker.exe can run in 64-bit mode on compatible systems.
    /// </summary>
    public static class WorkerEntryPoint
    {
        public static void Run(string[] args)
        {
            try
            {
#if DEBUG
                // Debugger.Launch();
#endif

                // Initialize Ninject (the DI framework)
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());
                var workerService = kernel.Get<WorkerService>();

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;

                // Set up a form for the worker process
                // A parent form is needed for some operations, namely 64-bit TWAIN scanning
                var form = new BackgroundForm();
                workerService.ParentForm = form;

                // Connect to the main NAPS2 process and listen for assigned work
                string pipeName = string.Format(WorkerManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                using (var host = new ServiceHost(workerService))
                {
                    host.AddServiceEndpoint(typeof (IWorkerService),
                        new NetNamedPipeBinding {ReceiveTimeout = TimeSpan.FromHours(24), SendTimeout = TimeSpan.FromHours(24)}, pipeName);
                    host.Open();
                    // Send a character to stdout to indicate that the process is ready for work
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
