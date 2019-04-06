using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config.Experimental;
using Xunit;

namespace NAPS2.Sdk.Tests.Config
{
    public class ConfigScopeTests : ContextualTexts
    {
        [Fact]
        public void StubProvider()
        {
            var provider = new StubConfigProvider<CommonConfig>(new CommonConfig
            {
                Culture = "fr",
                CheckForUpdates = true,
                PdfSettings =
                {
                    DefaultFileName = "name"
                }
            });
            Assert.Equal("fr", provider.Get(c => c.Culture));
            Assert.True(provider.Get(c => c.CheckForUpdates));
            Assert.Equal("name", provider.Get(c => c.PdfSettings.DefaultFileName));

            Assert.Null(provider.Get(c => c.ComponentsPath));
            Assert.False(provider.Get(c => c.DisableAutoSave));
        }

        [Fact]
        public void InternalDefaultsNotNullProps()
        {
            var config = InternalDefaults.GetCommonConfig();
            AssertPropNullOrNotNull(config, false);
        }

        [Fact]
        public void NewConfigNullProps()
        {
            var config = new CommonConfig();
            AssertPropNullOrNotNull(config, true);
        }

        private static void AssertPropNullOrNotNull(object config, bool shouldBeNull)
        {
            Assert.NotNull(config);
            foreach (var prop in config.GetType().GetProperties())
            {
                var value = prop.GetValue(config);
                if (prop.CustomAttributes.Any(x => typeof(ChildAttribute).IsAssignableFrom(x.AttributeType)))
                {
                    // Child, so recurse
                    AssertPropNullOrNotNull(value, shouldBeNull);
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
}
