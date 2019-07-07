using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
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
                CheckForUpdates = true,
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
            Assert.True(dst.CheckForUpdates);
            Assert.Equal("test_name", dst.PdfSettings.DefaultFileName);
            Assert.Equal("test_driver", dst.DefaultProfileSettings.DriverName);
            Assert.False(ReferenceEquals(src.PdfSettings, dst.PdfSettings));
            Assert.True(ReferenceEquals(src.DefaultProfileSettings, dst.DefaultProfileSettings));
        }

        [Fact]
        public void DoesNotOverwriteWithNull()
        {
            var dst = new CommonConfig
            {
                Culture = "fr",
                CheckForUpdates = true,
                PdfSettings =
                {
                    DefaultFileName = "test_name"
                },
                DefaultProfileSettings = new ScanProfile
                {
                    DriverName = "test_driver"
                }
            };
            var src = new CommonConfig();
            ConfigCopier.Copy(src, dst);
            Assert.Equal("fr", dst.Culture);
            Assert.True(dst.CheckForUpdates);
            Assert.Equal("test_name", dst.PdfSettings.DefaultFileName);
            Assert.Equal("test_driver", dst.DefaultProfileSettings.DriverName);
        }
    }
}
