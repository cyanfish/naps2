using System.Linq;
using NAPS2.Config;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Config;

public class CommonConfigTests : ContextualTexts
{
    [Fact]
    public void CanSerialize()
    {
        var config = InternalDefaults.GetCommonConfig();
        config.FormStates = config.FormStates.Add(new FormState { Name = "Test" });
        config.BackgroundOperations = config.BackgroundOperations.Add("abc");


        var xml = config.ToXml();
        var config2 = xml.FromXml<CommonConfig>();

        Assert.Single(config2.FormStates);
        Assert.Equal("Test", config2.FormStates[0].Name);
        Assert.Single(config2.BackgroundOperations);
        Assert.Equal("abc", config2.BackgroundOperations.Single());
    }

    [Fact]
    public void NoNulls()
    {
        var config = InternalDefaults.GetCommonConfig();
        foreach (var prop in typeof(CommonConfig).GetProperties())
        {
            Assert.NotNull(prop.GetValue(config));
        }
    }
}