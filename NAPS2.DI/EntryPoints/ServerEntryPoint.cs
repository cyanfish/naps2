using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ClientServer;
using NAPS2.DI.Modules;
using NAPS2.Logging;
using NAPS2.Util;
using NAPS2.WinForms;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI.EntryPoints
{
    /// <summary>
    /// The entry point for NAPS2.Server.exe, which exposes scanning devices over the network to other NAPS2 applications.
    /// </summary>
    public static class ServerEntryPoint
    {
        private const int DEFAULT_PORT = 33277;

        public static void Run(string[] args)
        {
            try
            {
                // Initialize Ninject (the DI framework)
                var kernel = new StandardKernel(new CommonModule(), new WinFormsModule());

                // Start a pending worker process
                WorkerManager.Init();

                // Set up basic application configuration
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += UnhandledException;
                TaskScheduler.UnobservedTaskException += UnhandledTaskException;

                // Set up a form for the server process
                var form = new BackgroundForm();
                Invoker.Current = form;

                int port = DEFAULT_PORT;
                foreach (var portArg in args.Where(a => a.StartsWith("/Port:", StringComparison.OrdinalIgnoreCase)))
                {
                    if (int.TryParse(portArg.Substring(6), out int parsedPort))
                    {
                        port = parsedPort;
                    }
                }

                new Thread(() => ServerDiscovery.ListenForBroadcast(port)) { IsBackground = true }.Start();

                // Listen for requests
                using (var host = new ServiceHost(typeof(ScanService)))
                {
                    var serverIcon = new ServerNotifyIcon(port, () => form.Close());
                    host.Opened += (sender, eventArgs) => serverIcon.Show();
                    host.Description.Behaviors.Add(new ServiceFactoryBehavior(() => kernel.Get<ScanService>()));
                    var binding = new NetTcpBinding
                    {
                        ReceiveTimeout = TimeSpan.FromHours(1),
                        SendTimeout = TimeSpan.FromHours(1),
                        Security =
                        {
                            Mode = SecurityMode.None
                        }
                    };
                    host.AddServiceEndpoint(typeof(IScanService), binding, $"net.tcp://0.0.0.0:{port}/NAPS2.Server");
                    host.Open();
                    try
                    {
                        Application.Run(form);
                    }
                    finally
                    {
                        serverIcon.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.FatalException("An error occurred that caused the server application to close.", ex);
                Environment.Exit(1);
            }
        }

        private static void UnhandledTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the server task to terminate.", e.Exception);
            e.SetObserved();
        }

        private static void UnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            Log.FatalException("An error occurred that caused the server to close.", e.Exception);
        }
    }
}
