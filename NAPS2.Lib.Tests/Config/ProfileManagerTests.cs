using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class ProfileManagerTests : ContextualTexts
{
    private readonly string _userPath;
    private readonly string _systemPath;
    private readonly ProfileManager _profileManager;

    public ProfileManagerTests()
    {
        _userPath = Path.Combine(FolderPath, Path.GetRandomFileName());
        _systemPath = Path.Combine(FolderPath, Path.GetRandomFileName());
        _profileManager = new ProfileManager(_userPath, _systemPath, false, false, false);
    }

    [Fact]
    public void SaveOneProfile()
    {
        var profile = new ScanProfile
        {
            Device = new ScanDevice("test_id", "test_name"),
            DisplayName = "A Profile"
        };
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), new Selectable<ScanProfile>());
        _profileManager.Save();

        var doc = XDocument.Load(_userPath);

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
    public void LoadOneProfile()
    {
        var xml = ProfileSerializerTestsData.SingleProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("test_driver", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(2, profiles[0].Version);
        Assert.Null(profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadVeryOldProfile()
    {
        var xml = ProfileSerializerTestsData.VeryOldProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("wia", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(ScanDpi.Dpi200, profiles[0].Resolution);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(0, profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadOldProfile()
    {
        var xml = ProfileSerializerTestsData.OldProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("wia", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(ScanDpi.Dpi200, profiles[0].Resolution);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(1, profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadOldTwainProfile()
    {
        var xml = ProfileSerializerTestsData.OldTwainProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("twain", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.True(profiles[0].UseNativeUI);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(1, profiles[0].UpgradedFrom);
    }
}