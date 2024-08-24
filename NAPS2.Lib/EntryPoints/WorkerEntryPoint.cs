using Autofac;
using Microsoft.Extensions.Logging;
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
    public static int Run(string[] args, Module imageModule, Action? run = null, Action? stop = null)
    {
            // Initialize Autofac (the DI framework)
            var container = AutoFacHelper.FromModules(
                new CommonModule(), imageModule, new WorkerModule(), new ContextModule());

            var logger = container.Resolve<ILogger>();
            var serviceImpl = container.Resolve<WorkerServiceImpl>();

            return CoreWorkerEntryPoint.Run(args, logger, serviceImpl, run, stop);
    }
}