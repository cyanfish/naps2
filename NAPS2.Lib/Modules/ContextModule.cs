using Autofac;
using NLog;

namespace NAPS2.Modules;

public class ContextModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        Log.Logger = new NLogLogger();
#if DEBUG
        Trace.Listeners.Add(new NLogTraceListener());
#endif
    }
}