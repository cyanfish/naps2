using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using UnhandledExceptionEventArgs = Eto.UnhandledExceptionEventArgs;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI.
/// </summary>
public static class MacEntryPoint
{
    public static int Run(string[] args)
    {
        if (args.Length > 0 && args[0] is "cli" or "console")
        {
            return ConsoleEntryPoint.Run(args.Skip(1).ToArray(), new MacModule());
        }
        if (args.Length > 0 && args[0] == "worker")
        {
            return WorkerEntryPoint.Run(args.Skip(1).ToArray(), new MacModule());
        }

        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), new MacModule(), new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Set up basic application configuration
        container.Resolve<CultureHelper>().SetCulturesFromConfig();
        TaskScheduler.UnobservedTaskException += UnhandledTaskException;
        Trace.Listeners.Add(new ConsoleTraceListener());

        Runtime.MarshalManagedException += (_, eventArgs) =>
        {
            Log.ErrorException("Marshalling managed exception", eventArgs.Exception);
            eventArgs.ExceptionMode = MarshalManagedExceptionMode.ThrowObjectiveCException;
        };
        Runtime.MarshalObjectiveCException += (_, eventArgs) =>
        {
            Log.Error($"Marshalling ObjC exception: {eventArgs.Exception.Description}");
        };

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init();

        // Show the main form
        var application = EtoPlatform.Current.CreateApplication();
        application.UnhandledException += UnhandledException;
        var formFactory = container.Resolve<IFormFactory>();
        var desktop = formFactory.Create<DesktopForm>();
        // Invoker.Current = new WinFormsInvoker(desktop.ToNative());

        application.Run(desktop);
        return 0;
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
        e.SetObserved();
    }

    private static void UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the application to close.",
            e.ExceptionObject as Exception ?? new Exception());
    }
}