using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Core;
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
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule(), new StaticDefaultsModule());

                // Expect a single argument, the parent process id
                if (args.Length != 1 || !int.TryParse(args[0], out int procId) || !IsProcessRunning(procId))
                {
                    return;
                }

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;
                TaskScheduler.UnobservedTaskException += UnhandledTaskException;

                // Set up a form for the worker process
                // A parent form is needed for some operations, namely 64-bit TWAIN scanning
                var form = new BackgroundForm();
                Invoker.Current = form;

                var (rootCert, rootPrivate) = SslHelper.GenerateRootCertificate();
                //var (cert, privateKey) = SslHelper.GenerateCertificateChain(rootCert, rootPrivate);
                var creds = new SslServerCredentials(new[] {new KeyCertificatePair(rootCert, rootPrivate)}, rootCert,
                    SslClientCertificateRequestType.RequestAndRequireAndVerify);

                // Connect to the main NAPS2 process and listen for assigned work
                Server server = new Server
                {
                    Services = { GrpcWorkerService.BindService(kernel.Get<GrpcWorkerServiceImpl>()) },
                    Ports = { new ServerPort("localhost", 0, creds) }
                };
                server.Start();
                try
                {
                    // Send the port to stdout
                    Console.WriteLine(server.Ports.First().BoundPort);
                    Application.Run(form);
                }
                finally
                {
                    server.ShutdownAsync().Wait();
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

        private static void UnhandledTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the worker task to terminate.", e.Exception);
            e.SetObserved();
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the worker to close.", e.Exception);
        }
    }
}
