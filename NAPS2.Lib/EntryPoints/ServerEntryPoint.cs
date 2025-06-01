using System.Threading;
using Autofac;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
    public static int Run(string[] args, Module imageModule, Module platformModule, Action<IContainer>? run = null)
    {
        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(new CommonModule(), imageModule, platformModule, new WorkerModule(),
            new StaticInitModule());

        TaskScheduler.UnobservedTaskException += UnhandledTaskException;

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init(container.Resolve<ScanningContext>());

        run ??= _ =>
        {
            var reset = new ManualResetEvent(false);
            var sharedDeviceManager = container.Resolve<ISharedDeviceManager>();
            sharedDeviceManager.StartSharing();
            var processCoordinator = container.Resolve<ProcessCoordinator>();
            processCoordinator.StartServer(new ProcessCoordinatorServiceImpl(reset));
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                // TODO: Actually wait for sharing to stop
                sharedDeviceManager.StopSharing();
                processCoordinator.KillServer();
            };
            Console.CancelKeyPress += (_, _) => reset.Set();
            reset.WaitOne();
            // TODO: Why is this needed?
            Environment.Exit(0);
        };
        run(container);

        return 0;
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the server task to terminate.", e.Exception);
        e.SetObserved();
    }

    private class ProcessCoordinatorServiceImpl(ManualResetEvent reset)
        : ProcessCoordinatorService.ProcessCoordinatorServiceBase
    {
        public override Task<Empty> StopSharingServer(StopSharingServerRequest request, ServerCallContext context)
        {
            reset.Set();
            return Task.FromResult(new Empty());
        }
    }
}