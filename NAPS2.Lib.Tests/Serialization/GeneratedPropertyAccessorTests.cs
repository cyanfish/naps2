using System.Reflection;
using NAPS2.Config;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Lib.Tests.Serialization;

public class GeneratedPropertyAccessorTests
{
    public static IEnumerable<object[]> GeneratedTypes =>
        typeof(CommonConfig).Assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<GeneratePropertyAccessorsAttribute>() != null)
            .Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void AllPropertiesHaveGeneratedAccessors(Type type)
    {
        foreach (var prop in GetProperties(type))
        {
            if (prop.GetMethod is { IsPublic: true })
            {
                Assert.False(PropertyAccessors.GetGetter(prop).Target is PropertyInfo, $"{type.Name}.{prop.Name} getter");
            }
            if (prop.SetMethod is { IsPublic: true })
            {
                Assert.False(PropertyAccessors.GetSetter(prop).Target is PropertyInfo, $"{type.Name}.{prop.Name} setter");
            }
        }
    }

    [Theory]
    [MemberData(nameof(GeneratedTypes))]
    public void GeneratedAccessorsMatchReflection(Type type)
    {
        var source = Activator.CreateInstance(type, true)!;
        var target = Activator.CreateInstance(type, true)!;
        foreach (var prop in GetProperties(type).Where(x => x.GetMethod != null && x.SetMethod != null))
        {
            var value = PropertyAccessors.GetGetter(prop)(source);
            Assert.Equal(prop.GetValue(source), value);
            PropertyAccessors.GetSetter(prop)(target, value);
            Assert.Equal(value, prop.GetValue(target));
        }
    }

    private static PropertyInfo[] GetProperties(Type type) =>
        type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
}
