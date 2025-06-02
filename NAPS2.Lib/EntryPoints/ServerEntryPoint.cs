using Autofac;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Modules;
using NAPS2.Remoting;
using NAPS2.Remoting.Server;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2 running as a server for scanner sharing.
/// </summary>
public static class ServerEntryPoint
{
    public static int Run(string[] args, Module imageModule, Module platformModule)
    {
        // Initialize Autofac (the DI framework)
        var container =
            AutoFacHelper.FromModules(new CommonModule(), imageModule, platformModule, new StaticInitModule());

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init(container.Resolve<ScanningContext>());

        container.Resolve<CultureHelper>().SetCulturesFromConfig();

        // We need to set up an Eto application in order to display a tray indicator
        var application = EtoPlatform.Current.CreateApplication();
        application.Initialized += (_, _) =>
        {
            var sharedDeviceManager = container.Resolve<ISharedDeviceManager>();
            var processCoordinator = container.Resolve<ProcessCoordinator>();
            var trayIndicator = container.Resolve<ServerTrayIndicator>();

            void Stop(bool unregister = false)
            {
                processCoordinator.KillServer();
                sharedDeviceManager.StopSharing();
                if (unregister)
                {
                    // If the user manually stops the background app, treat it the same as if they unchecked
                    // "Share even when NAPS2 is closed".
                    // We need to do this in a separate process as on Mac/Linux this process will die as soon as the service
                    // is unregistered and the full cleanup won't happen.
                    Process.Start(AssemblyHelper.EntryFile, "/UnregisterSharingService").WaitForExit(5000);
                }
                application.Quit();
            }

            // Start the actual sharing server
            sharedDeviceManager.StartSharing();
            // Listen for the StopSharingServer event, which is sent if the user unchecks
            // "Share even when NAPS2 is closed"
            processCoordinator.StartServer(new ProcessCoordinatorServiceImpl(() => Stop()));

            // Set up and show the tray indicator, which has a single "Stop Scanner Sharing" menu item
            trayIndicator.StopClicked += (_, _) => Stop(true);
            trayIndicator.Show();

            // If the server was started on the command-line, allow Ctrl+C to terminate it
            Console.CancelKeyPress += (_, _) => Stop();
        };
        Invoker.Current = new EtoInvoker(application);
        application.Run();

        return 0;
    }

    private class ProcessCoordinatorServiceImpl(Action stop)
        : ProcessCoordinatorService.ProcessCoordinatorServiceBase
    {
        public override Task<StopSharingServerResponse> StopSharingServer(StopSharingServerRequest request,
            ServerCallContext context)
        {
            stop();
            return Task.FromResult(new StopSharingServerResponse { Stopped = true });
        }
    }
}