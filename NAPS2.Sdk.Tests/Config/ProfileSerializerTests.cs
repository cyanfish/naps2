using NAPS2.Scan;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Config;

public class ProfileSerializerTests : ContextualTexts
{
    [Fact]
    public void Serialization()
    {
        // We don't want this test to break whenever we add a new profile field, so only doing partial verification
        var serializer = new ProfileSerializer();
        var profile = new ScanProfile
        {
            Device = new ScanDevice("test_id", "test_name"),
            DisplayName = "A Profile"
        };
        var list = new List<ScanProfile> { profile };

        var doc = XDocument.Parse(serializer.SerializeToString(list));

        var root = doc.Root;
        Assert.NotNull(root);
        Assert.Equal("ArrayOfScanProfile", root.Name);
        var profileEl = root.Elements("ScanProfile").Single();
        Assert.Equal("test_id", profileEl.Element("Device")?.Element("ID")?.Value);
        Assert.Equal("A Profile", profileEl.Element("DisplayName")?.Value);
        // XmlIgnore elements
        Assert.Empty(profileEl.Elements("IsLocked"));
        Assert.Empty(profileEl.Elements("IsDeviceLocked"));
        Assert.Empty(profileEl.Elements("UpgradedFrom"));
    }

    [Fact]
    public void Deserialization()
    {
        var serializer = new ProfileSerializer();
        var xml = ProfileSerializerTestsData.SingleProfile;

        var profiles = serializer.DeserializeFromString(xml);
        var profile = profiles.Single();

        Assert.Equal("test_driver", profile.DriverName);
        Assert.Equal("test_id", profile.Device.ID);
        Assert.Equal(2, profile.Version);
        Assert.Null(profile.UpgradedFrom);
    }

    [Fact]
    public void DeserializeVeryOldProfile()
    {
        var serializer = new ProfileSerializer();
        var xml = ProfileSerializerTestsData.VeryOldProfile;

        var profiles = serializer.DeserializeFromString(xml);
        var profile = profiles.Single();

        Assert.Equal("wia", profile.DriverName);
        Assert.Equal("test_id", profile.Device.ID);
        Assert.Equal(ScanDpi.Dpi200, profile.Resolution);
        Assert.Equal(2, profile.Version);
        Assert.Equal(0, profile.UpgradedFrom);
    }

    [Fact]
    public void DeserializeOldProfile()
    {
        var serializer = new ProfileSerializer();
        var xml = ProfileSerializerTestsData.OldProfile;

        var profiles = serializer.DeserializeFromString(xml);
        var profile = profiles.Single();

        Assert.Equal("wia", profile.DriverName);
        Assert.Equal("test_id", profile.Device.ID);
        Assert.Equal(ScanDpi.Dpi200, profile.Resolution);
        Assert.Equal(2, profile.Version);
        Assert.Equal(1, profile.UpgradedFrom);
    }

    [Fact]
    public void DeserializeOldTwainProfile()
    {
        var serializer = new ProfileSerializer();
        var xml = ProfileSerializerTestsData.OldTwainProfile;

        var profiles = serializer.DeserializeFromString(xml);
        var profile = profiles.Single();

        Assert.Equal("twain", profile.DriverName);
        Assert.Equal("test_id", profile.Device.ID);
        Assert.True(profile.UseNativeUI);
        Assert.Equal(2, profile.Version);
        Assert.Equal(1, profile.UpgradedFrom);
    }
}