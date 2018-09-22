using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.DI.Modules;
using NAPS2.Util;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI.EntryPoints
{
    /// <summary>
    /// The entry point for NAPS2.Server.exe, which exposes scanning devices over the network to other NAPS2 applications.
    /// </summary>
    public static class ServerEntryPoint
    {
        public static void Run(string[] args)
        {
            try
            {
                // Initialize Ninject (the DI framework)
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;

                // Set up a form for the server process
                var form = new BackgroundForm();
                Invoker.Current = form;

                // TODO: Listen for requests
            }
            catch (Exception ex)
            {
                Log.FatalException("An error occurred that caused the server application to close.", ex);
                Environment.Exit(1);
            }
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            Log.FatalException("An error occurred that caused the server to close.", threadExceptionEventArgs.Exception);
        }
    }
}
