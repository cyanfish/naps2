using System.Threading;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.WinForms;
using NAPS2.Modules;
using NAPS2.Platform.Windows;
using NAPS2.Remoting.Worker;
using NAPS2.WinForms;
using Ninject;
using Application = System.Windows.Forms.Application;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI.
/// </summary>
public static class WinFormsEntryPoint
{
    public static void Run(string[] args)
    {
        // Initialize Ninject (the DI framework)
        var kernel = new StandardKernel(new CommonModule(), new GdiModule(), new WinFormsModule(),
            new RecoveryModule(), new ContextModule());

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
        var application = new Eto.Forms.Application(Eto.Platforms.WinForms);
        var formFactory = kernel.Get<IFormFactory>();
        var desktop = formFactory.Create<DesktopForm>();
        Invoker.Current = new WinFormsInvoker(desktop.ToNative());

        application.Run(desktop);
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