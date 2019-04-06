using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config.Experimental;
using NAPS2.Scan;
using Xunit;

namespace NAPS2.Sdk.Tests.Config
{
    public class ConfigCopierTests
    {
        [Fact]
        public void Copies()
        {
            var src = new CommonConfig
            {
                Culture = "fr",
                PdfSettings =
                {
                    DefaultFileName = "test_name"
                },
                DefaultProfileSettings = new ScanProfile
                {
                    DriverName = "test_driver"
                }
            };
            var dst = new CommonConfig();
            ConfigCopier.Copy(src, dst);
            Assert.Equal("fr", dst.Culture);
            Assert.Equal("test_name", dst.PdfSettings.DefaultFileName);
            Assert.Equal("test_driver", dst.DefaultProfileSettings.DriverName);
            Assert.False(ReferenceEquals(src.PdfSettings, dst.PdfSettings));
            Assert.True(ReferenceEquals(src.DefaultProfileSettings, dst.DefaultProfileSettings));
        }
    }
}
