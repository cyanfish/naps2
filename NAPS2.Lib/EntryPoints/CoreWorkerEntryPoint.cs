using System.Threading;
using GrpcDotNetNamedPipes;
using Microsoft.Extensions.Logging;
using NAPS2.Remoting.Worker;

namespace NAPS2.EntryPoints;

/// <summary>
/// A stripped-down version of WorkerEntryPoint with limited dependencies.
/// </summary>
public static class CoreWorkerEntryPoint
{
    private static readonly TimeSpan ParentCheckInterval = TimeSpan.FromSeconds(10);

    internal static int Run(string[] args, ILogger logger, WorkerServiceImpl serviceImpl, Action? run = null, Action? stop = null)
    {
        try
        {
            // Expect a single argument, the parent process id
            if (args.Length != 1 || !int.TryParse(args[0], out int procId) || !IsProcessRunning(procId))
            {
                return 1;
            }

            void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
            {
                logger.LogError(e.Exception, "An error occurred that caused the worker task to terminate.");
                e.SetObserved();
            }
            TaskScheduler.UnobservedTaskException += UnhandledTaskException;

            var flag = new ManualResetEvent(false);
            run ??= () => flag.WaitOne();
            stop ??= () => flag.Set();

            // Connect to the main NAPS2 process and listen for assigned work
            var server =
                new NamedPipeServer(string.Format(WorkerFactory.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id));
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
            logger.LogError(ex, "An error occurred that caused the worker application to close.");
            return 1;
        }
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
}