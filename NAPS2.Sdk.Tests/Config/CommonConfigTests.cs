using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Config
{
    public class CommonConfigTests : ContextualTexts
    {
        [Fact]
        public void CanSerialize()
        {
            var config = InternalDefaults.GetCommonConfig();
            config.FormStates.Add(new FormState { Name = "Test" });
            
            var xml = config.ToXml();
            var config2 = xml.FromXml<CommonConfig>();

            Assert.Single(config2.FormStates);
            Assert.Equal("Test", config2.FormStates[0].Name);
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
}
