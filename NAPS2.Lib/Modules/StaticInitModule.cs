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
        builder.RegisterBuildCallback(ctx =>
        {
            Log.Logger = ctx.Resolve<ILogger>();
        });
        Trace.Listeners.Add(new NLogTraceListener());
    }
}