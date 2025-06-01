using Autofac;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Modules;

/// <summary>
/// Worker-specific module used by WorkerEntryPoint.
/// </summary>
public class WorkerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Bindings for ITwainController as used by WorkerServiceImpl
#if MAC
        builder.RegisterType<StubTwainController>().As<ITwainController>();
#elif NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            builder.RegisterType<LocalTwainController>().As<ITwainController>();
        }
        else
        {
            builder.RegisterType<StubTwainController>().As<ITwainController>();
        }
#else
        builder.RegisterType<LocalTwainController>().As<ITwainController>();
#endif
    }
}