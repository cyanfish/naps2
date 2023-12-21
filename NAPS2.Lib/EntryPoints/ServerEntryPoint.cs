using System.Threading;
using Autofac;
using NAPS2.Modules;
using NAPS2.Remoting.Server;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2 running as a server for scanner sharing.
/// </summary>
public static class ServerEntryPoint
{
    public static int Run(string[] args, Module imageModule, Action<IContainer>? run = null)
    {
        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), imageModule, new WorkerModule(), new ContextModule());

        TaskScheduler.UnobservedTaskException += UnhandledTaskException;

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init(container.Resolve<ScanningContext>());

        run ??= _ =>
        {
            var sharedDeviceManager = container.Resolve<ISharedDeviceManager>();
            sharedDeviceManager.StartSharing();
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                // TODO: Actually wait for sharing to stop
                sharedDeviceManager.StopSharing();
            };
            var reset = new ManualResetEvent(false);
            Console.CancelKeyPress += (_, _) => reset.Set();
            reset.WaitOne();
        };
        run(container);

        return 0;
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the server task to terminate.", e.Exception);
        e.SetObserved();
    }
}