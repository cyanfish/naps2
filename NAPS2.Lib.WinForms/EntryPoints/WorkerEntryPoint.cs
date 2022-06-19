using System.Text;
using System.Threading;
using System.Windows.Forms;
using GrpcDotNetNamedPipes;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;
using NAPS2.WinForms;
using Ninject;

namespace NAPS2.EntryPoints;

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
            var kernel = new StandardKernel(new CommonModule(), new WinFormsModule(), new ContextModule());
            // Verify that the recovery is always initialized by the parent process before creating images
            // TODO: Replace this with something maybe
            // kernel.Get<ImageContext>().UseFileStorage(new NotToBeUsedStorageManager());

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

            // Connect to the main NAPS2 process and listen for assigned work
            var server = new NamedPipeServer(string.Format(WorkerFactory.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id));
            WorkerService.BindService(server.ServiceBinder, kernel.Get<WorkerServiceImpl>());
            server.Start();
            try
            {
                Console.WriteLine(@"ready");
                Application.Run(form);
            }
            finally
            {
                server.Kill();
            }
        }
        catch (Exception ex)
        {
            Console.Write(@"error");
            Log.FatalException("An error occurred that caused the worker application to close.", ex);
            Environment.Exit(1);
        }
    }

    private static string ReadEncodedString()
    {
        string input = Console.ReadLine() ?? throw new InvalidOperationException("No input");
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
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