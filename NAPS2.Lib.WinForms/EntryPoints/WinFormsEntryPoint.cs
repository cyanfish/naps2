using System.Threading;
using System.Windows.Forms;
using NAPS2.Modules;
using NAPS2.Platform.Windows;
using NAPS2.Remoting.Worker;
using NAPS2.WinForms;
using Ninject;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI.
/// </summary>
public static class WinFormsEntryPoint
{
    public static void Run(string[] args)
    {
        // Initialize Ninject (the DI framework)
        var kernel = new StandardKernel(new CommonModule(), new WinFormsModule(), new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Parse the command-line arguments and see if we're doing something other than displaying the main form
        var lifecycle = kernel.Get<WindowsApplicationLifecycle>();
        lifecycle.ParseArgs(args);
        lifecycle.ExitIfRedundant();

        // Start a pending worker process
        kernel.Get<IWorkerFactory>().Init();

        // Set up basic application configuration
        kernel.Get<CultureHelper>().SetCulturesFromConfig();
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

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
        e.SetObserved();
    }

    private static void UnhandledException(object? sender, ThreadExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the application to close.", e.Exception);
    }
}