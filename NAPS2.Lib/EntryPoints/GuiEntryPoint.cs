using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for the main NAPS2 executable.
/// </summary>
public static class GuiEntryPoint
{
    public static int Run(string[] args, Module imageModule, Module platformModule)
    {
        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), imageModule, platformModule, new GuiModule(), new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Parse the command-line arguments and see if we're doing something other than displaying the main form
        var lifecycle = container.Resolve<ApplicationLifecycle>();
        lifecycle.ParseArgs(args);
        lifecycle.ExitIfRedundant();

        EtoPlatform.Current.InitializeForegroundApp();

        // Set up basic application configuration
        container.Resolve<CultureHelper>().SetCulturesFromConfig();
        TaskScheduler.UnobservedTaskException += UnhandledTaskException;
        Trace.Listeners.Add(new ConsoleTraceListener());

        // Start a pending worker process
        container.Resolve<IWorkerFactory>().Init(container.Resolve<ScanningContext>());

        // Show the main form
        var application = EtoPlatform.Current.CreateApplication();
        application.UnhandledException += UnhandledException;
        Invoker.Current = new EtoInvoker(application);
        var formFactory = container.Resolve<IFormFactory>();
        var desktop = formFactory.Create<DesktopForm>();

        application.Run(desktop);
        return 0;
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
        e.SetObserved();
    }

    private static void UnhandledException(object? sender, Eto.UnhandledExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the application to close.",
            e.ExceptionObject as Exception ?? new Exception());
    }
}