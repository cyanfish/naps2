using Autofac;
using NAPS2.Modules;
using Xunit;

namespace NAPS2.Lib.Tests.Modules;

public class ConstructorInvokersTests
{
    [Fact]
    public void AutofacUsesInvokers()
    {
        var builder = new ContainerBuilder();
        ConstructorInvokers.Register(builder);
        builder.RegisterType<Dependency>().AsSelf();
        builder.RegisterType<Service>().AsSelf();
        var container = builder.Build();

        Assert.True(container.Resolve<Service>().CreatedByInvoker);
    }

    [Fact]
    public void AutofacUsesInvokersForImplicitRegistrations()
    {
        var builder = new ContainerBuilder();
        ConstructorInvokers.Register(builder);
        builder.RegisterSource<Autofac.Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource>();
        var container = builder.Build();

        Assert.True(container.Resolve<ImplicitService>().CreatedByInvoker);
    }

    [Fact]
    public void ConvertsPrimitiveParameters()
    {
        var builder = new ContainerBuilder();
        ConstructorInvokers.Register(builder);
        builder.RegisterType<Dependency>().AsSelf();
        builder.RegisterType<Service>().AsSelf();
        var container = builder.Build();

        Assert.Equal(5, container.Resolve<Service>(new NamedParameter("count", 5L)).Count);
    }

    private static bool IsCreatedByInvoker() =>
        new StackTrace().GetFrames().Any(f => f.GetMethod()?.DeclaringType?.DeclaringType == typeof(ConstructorInvokers));

    public class Dependency;

    public class Service
    {
        public Service(Dependency dependency) : this(dependency, 0)
        {
        }

        public Service(Dependency dependency, int count)
        {
            Count = count;
            CreatedByInvoker = IsCreatedByInvoker();
        }

        public int Count { get; }
        public bool CreatedByInvoker { get; }
    }

    public class ImplicitService(Dependency dependency)
    {
        public Dependency Dependency { get; } = dependency;
        public bool CreatedByInvoker { get; } = IsCreatedByInvoker();
    }
}
