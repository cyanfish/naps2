using Autofac;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NAPS2.Modules;

/// <summary>
/// Static class initialization module used by all entry points except tests.
/// </summary>
public class StaticInitModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterBuildCallback(ctx => { Log.Logger = ctx.Resolve<ILogger>(); });
        Trace.Listeners.Add(new NLogTraceListener());
        TaskScheduler.UnobservedTaskException += UnhandledTaskException;
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
        e.SetObserved();
    }
}