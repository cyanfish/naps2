using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using NAPS2.DI.Modules;
using NAPS2.Logging;
using NAPS2.Util;
using NAPS2.WinForms;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI.EntryPoints
{
    /// <summary>
    /// The entry point for NAPS2.Worker.exe, an off-process worker.
    ///
    /// NAPS2.Worker.exe runs in 32-bit mode for compatibility with 32-bit TWAIN drivers.
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

                // Expect a single argument, the parent process id
                if (args.Length != 1 || !int.TryParse(args[0], out int procId) || !IsProcessRunning(procId))
                {
                    return;
                }

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;

                // Set up a form for the worker process
                // A parent form is needed for some operations, namely 64-bit TWAIN scanning
                var form = new BackgroundForm();
                Invoker.Current = form;

                // Connect to the main NAPS2 process and listen for assigned work
                string pipeName = string.Format(WorkerManager.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id);
                using (var host = new ServiceHost(typeof(WorkerService)))
                {
                    host.Description.Behaviors.Add(new ServiceFactoryBehavior(() => kernel.Get<WorkerService>()));
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

        private static bool IsProcessRunning(int procId)
        {
            try
            {
                var proc = Process.GetProcessById(procId);
                return !proc.HasExited;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            Log.FatalException("An error occurred that caused the worker to close.", threadExceptionEventArgs.Exception);
        }
    }
}
