using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Util.Cache;

namespace NAPS2.Modules;

/// <summary>
/// Supplies Autofac with constructor invokers that use reflection, so it doesn't compile an expression tree for each
/// constructor, which has a significant startup cost.
/// <para>
/// This makes sense for the NAPS2 scenario where most classes are only created once or twice. And since .NET 7,
/// reflection emits a fast invoker after the first call anyway, so we don't lose much sustained performance. See
/// https://github.com/dotnet/runtime/pull/67917.
/// </para>
/// </summary>
public static class ConstructorInvokers
{
    // The cache that Autofac's ConstructorBinder uses for invokers, only compiling one when it's missing
    private static readonly ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>> Cache =
        ReflectionCacheSet.Shared.GetOrCreateCache<ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>>>(
            "ConstructorBinderFactory");

    public static void Register(ContainerBuilder builder)
    {
        builder.ComponentRegistryBuilder.Registered += (_, e) =>
        {
            if (e.ComponentRegistration.Activator is ReflectionActivator activator)
            {
                foreach (var ctor in activator.ConstructorFinder.FindConstructors(activator.LimitType))
                {
                    Cache.TryAdd(ctor, CreateInvoker(ctor));
                }
            }
        };
    }

    private static Func<object?[], object> CreateInvoker(ConstructorInfo ctor)
    {
        var parameters = ctor.GetParameters();
        return args =>
        {
            for (int i = 0; i < args.Length; i++)
            {
                var type = parameters[i].ParameterType;
                // Autofac's compiled invokers convert primitive values (e.g. from parameters passed to Resolve)
                if (type.IsPrimitive && args[i] != null && args[i]!.GetType() != type)
                {
                    args[i] = Convert.ChangeType(args[i], type);
                }
            }
            return ctor.Invoke(args);
        };
    }
}