using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class InternalDefaultsTests
{
    [Fact]
    public void NotNullProps()
    {
        var config = InternalDefaults.GetCommonConfig();
        AssertPropNullOrNotNull(config, false, "");
    }

    private static void AssertPropNullOrNotNull(object config, bool shouldBeNull, string path)
    {
        Assert.True(config != null, path);
        foreach (var prop in config.GetType().GetProperties())
        {
            var value = prop.GetValue(config);
            if (prop.CustomAttributes.Any(x => typeof(ChildAttribute).IsAssignableFrom(x.AttributeType)))
            {
                // Child, so recurse
                AssertPropNullOrNotNull(value, shouldBeNull, $"{path}{prop.Name}.");
            }
            else
            {
                if (shouldBeNull)
                {
                    Assert.True(value == null, $"{prop.DeclaringType?.Name}.{prop.Name} == null");
                }
                else
                {
                    Assert.True(value != null, $"{prop.DeclaringType?.Name}.{prop.Name} != null");
                }
            }
        }
    }
}