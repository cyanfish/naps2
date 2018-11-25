using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// The entry point logic for NAPS2.exe, the NAPS2 GUI.
    /// </summary>
    public static class WinFormsEntryPoint
    {
        public static void Run(string[] args)
        {
            // Initialize Ninject (the DI framework)
            var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());

            Paths.ClearTemp();

            // Parse the command-line arguments and see if we're doing something other than displaying the main form
            var lifecycle = kernel.Get<Lifecycle>();
            lifecycle.ParseArgs(args);
            lifecycle.ExitIfRedundant();

            // Start a pending worker process
            WorkerManager.Init();

            // Set up basic application configuration
            kernel.Get<CultureInitializer>().InitCulture(Thread.CurrentThread);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += UnhandledException;
            TaskScheduler.UnobservedTaskException += UnhandledTaskException;

            // Show the main form
            var formFactory = kernel.Get<IFormFactory>();
            var desktop = formFactory.Create<FDesktop>();
            Invoker.Current = desktop;
            Application.Run(desktop);
        }

        private static void UnhandledTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
            e.SetObserved();
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the application to close.", e.Exception);
        }
    }
}
