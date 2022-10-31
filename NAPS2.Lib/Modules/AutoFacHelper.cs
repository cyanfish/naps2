using Autofac;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace NAPS2.Modules;

public class AutoFacHelper
{
    public static IContainer FromModules(params IModule[] modules)
    {
        var builder = new ContainerBuilder();
        builder.RegisterSource<AnyConcreteTypeNotAlreadyRegisteredSource>();
        foreach (var module in modules)
        {
            builder.RegisterModule(module);
        }
        return builder.Build();
    }
}