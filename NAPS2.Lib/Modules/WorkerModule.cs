﻿using Autofac;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Modules;

public class WorkerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(ctx => new ScanningContext(ctx.Resolve<ImageContext>()));
#if MAC
        builder.RegisterType<StubTwainSessionController>().As<ITwainSessionController>();
#else
        builder.RegisterType<LocalTwainSessionController>().As<ITwainSessionController>();
#endif
    }
}