using System.Text;
using System.Threading;
using Autofac;
using GrpcDotNetNamedPipes;
using NAPS2.Modules;
using NAPS2.Remoting.Worker;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point for NAPS2.Worker.exe, an off-process worker.
///
/// NAPS2.Worker.exe runs in 32-bit mode for compatibility with 32-bit TWAIN drivers.
/// </summary>
public static class WorkerEntryPoint
{
    private static readonly TimeSpan ParentCheckInterval = TimeSpan.FromSeconds(10);

    public static int Run(string[] args, Module imageModule, Action? run = null, Action? stop = null)
    {
        try
        {
#if DEBUG
            // Debugger.Launch();
#endif

            // Initialize Autofac (the DI framework)
            var container = AutoFacHelper.FromModules(
                new CommonModule(), imageModule, new WorkerModule(), new ContextModule());

            // Expect a single argument, the parent process id
            if (args.Length != 1 || !int.TryParse(args[0], out int procId) || !IsProcessRunning(procId))
            {
                return 1;
            }

            TaskScheduler.UnobservedTaskException += UnhandledTaskException;

            var flag = new ManualResetEvent(false);
            run ??= () => flag.WaitOne();
            stop ??= () => flag.Set();

            // Connect to the main NAPS2 process and listen for assigned work
            var server =
                new NamedPipeServer(string.Format(WorkerFactory.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id));
            var serviceImpl = container.Resolve<WorkerServiceImpl>();
            serviceImpl.OnStop += (_, _) => stop();
            using var parentCheckTimer = new Timer(_ =>
            {
                // The Job object created by the parent is supposed to kill the child processes,
                // but it can have issues (especially on Windows 7). This is a backup to avoid leftover workers.
                if (!IsProcessRunning(procId))
                {
                    serviceImpl.Stop();
                }
            }, null, TimeSpan.Zero, ParentCheckInterval);
            WorkerService.BindService(server.ServiceBinder, serviceImpl);
            server.Start();
            try
            {
                Console.WriteLine(@"ready");
                run();
            }
            finally
            {
                server.Kill();
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Write(@"error");
            Log.FatalException("An error occurred that caused the worker application to close.", ex);
            return 1;
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

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the worker task to terminate.", e.Exception);
        e.SetObserved();
    }
}