using Autofac;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NAPS2.Modules;

public class ContextModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterBuildCallback(ctx =>
        {
            Log.Logger = ctx.Resolve<ILogger>();
        });
#if DEBUG
        Trace.Listeners.Add(new NLogTraceListener());
#endif
    }
}