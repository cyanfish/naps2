using System.Reflection;
using System.Xml.Serialization;
using NAPS2.Config.ObsoleteTypes;
using NAPS2.Pdf;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Lib.Tests.Serialization;

public class GeneratedPropertyAccessorTests
{
    public static IEnumerable<object[]> GeneratedTypes =>
        new[] { typeof(CommonConfig).Assembly, typeof(PdfEncryption).Assembly, typeof(Transform).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttribute<GeneratePropertyAccessorsAttribute>() != null)
            .Select(type => new object[] { type });

    [Fact]
    public void SerializedTypesHaveGeneratedAccessors()
    {
        // The types that NAPS2 serializes (config, profiles, recovery index), and everything they contain
        Type[] roots = [typeof(CommonConfig), typeof(ScanProfile), typeof(RecoveryIndex), typeof(AppConfigV0),
            typeof(UserConfigV0)];
        var visited = new HashSet<Type>();
        var missing = new List<string>();

        void Visit(Type type)
        {
            if (!visited.Add(type))
            {
                return;
            }
            foreach (var typeArg in type.IsGenericType ? type.GetGenericArguments() : [])
            {
                Visit(typeArg);
            }
            if (type.GetElementType() is { } elementType)
            {
                Visit(elementType);
            }
            if (type.IsEnum || type.Assembly.GetName().Name?.StartsWith("NAPS2") != true ||
                TypeInfoAccess.HasCustomSerializer(type))
            {
                return;
            }
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(x => x.GetMethod != null && x.SetMethod != null &&
                            x.GetCustomAttribute<XmlIgnoreAttribute>() == null)
                .ToList();
            if (props.Any() && type.GetCustomAttribute<GeneratePropertyAccessorsAttribute>() == null)
            {
                missing.Add(type.FullName!);
            }
            foreach (var prop in props)
            {
                Visit(prop.PropertyType);
            }
            if (type.BaseType != null)
            {
                Visit(type.BaseType);
            }
            // Subtypes that can be serialized in place of this type (e.g. Transform subclasses)
            foreach (var knownType in TypeInfoAccess.GetSubtypes(type))
            {
                Visit(knownType);
            }
        }

        foreach (var root in roots)
        {
            Visit(root);
        }
        Assert.True(missing.Count == 0, $"Missing [GeneratePropertyAccessors]: {string.Join(", ", missing)}");
    }

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

    private class TypeInfoAccess : NAPS2.Serialization.XmlSerializer
    {
        public static bool HasCustomSerializer(Type type) => GetTypeInfo(type).CustomSerializer != null;

        public static IEnumerable<Type> GetSubtypes(Type type) =>
            GetTypeInfo(type).KnownTypesByElementName.Values.ToList();
    }
}
