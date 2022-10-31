using Autofac;
using NAPS2.Scan;
using NLog;

namespace NAPS2.Modules;

public class ContextModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterBuildCallback(ctx =>
        {
            ctx.Resolve<ScanningContext>().TempFolderPath = Paths.Temp;
            ctx.Resolve<ScanningContext>().RecoveryPath = Paths.Recovery;
        });

        Log.Logger = new NLogLogger();
#if DEBUG
        Trace.Listeners.Add(new NLogTraceListener());
#endif
    }
}