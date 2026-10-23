using NAPS2.Update;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NAPS2.Lib.Tests.Update;

public class UpdateCheckerTests
{
    private const string SHA = "qAXlGbYyd0IClwRqyY8v5PUOQzaKdL9HejEy4Ou8wUY=";
    private const string SIG = "AQID";

    [Fact]
    public void ReturnsNewerVersion()
    {
        var json = Json(Release("8.1.0"));

        var update = UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe");

        Assert.NotNull(update);
        Assert.Equal("8.1.0", update.Name);
        Assert.Equal("https://example.com/8.1.0.exe", update.DownloadUrl);
        Assert.Equal(Convert.FromBase64String(SHA), update.Sha256);
        Assert.Equal(new byte[] { 1, 2, 3 }, update.Signature256);
    }

    [Theory]
    [InlineData("8.0.0")]
    [InlineData("7.9.9")]
    public void IgnoresSameOrOlderVersion(string releaseVersion)
    {
        var json = Json(Release(releaseVersion));

        Assert.Null(UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe"));
    }

    [Fact]
    public void ReturnsFirstEligibleRelease()
    {
        var json = Json(Release("7.0.0"), Release("8.2.0"), Release("8.1.0"));

        var update = UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe");

        Assert.Equal("8.2.0", update?.Name);
    }

    [Fact]
    public void ParsesBetaVersionNames()
    {
        Assert.Equal("8.0b3", UpdateChecker.GetUpdateInfo(Json(Release("8.0b3")), new Version(8, 0, 2), "exe")?.Name);
        Assert.Null(UpdateChecker.GetUpdateInfo(Json(Release("8.0b3")), new Version(8, 0, 3), "exe"));
    }

    [Fact]
    public void RespectsMinimumVersionRequirement()
    {
        var json = Json(Release("9.0.0", gte: "8.5.0"), Release("8.6.0"));

        Assert.Equal("8.6.0", UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe")?.Name);
        Assert.Equal("9.0.0", UpdateChecker.GetUpdateInfo(json, new Version(8, 5, 0), "exe")?.Name);
    }

    [Fact]
    public void SelectsFileByExtension()
    {
        var json = Json(Release("8.1.0", ext: "msi"), Release("8.0.1", ext: "zip"));

        Assert.Equal("https://example.com/8.1.0.msi",
            UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "msi")?.DownloadUrl);
        Assert.Equal("https://example.com/8.0.1.zip",
            UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "zip")?.DownloadUrl);
        Assert.Null(UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe"));
    }

    [Fact]
    public void SkipsReleasesWithoutHashOrSignature()
    {
        var json = Json(
            Release("8.3.0", sha: null),
            Release("8.2.0", sig: null),
            Release("8.1.0"));

        Assert.Equal("8.1.0", UpdateChecker.GetUpdateInfo(json, new Version(8, 0, 0), "exe")?.Name);
    }

    [Fact]
    public void NoReleases()
    {
        Assert.Null(UpdateChecker.GetUpdateInfo(Json(), new Version(8, 0, 0), "exe"));
    }

    private static JObject Json(params JObject[] releases) =>
        new() { ["versions"] = new JArray(releases.Cast<object>().ToArray()) };

    private static JObject Release(string name, string? gte = null, string ext = "exe", string? sha = SHA,
        string? sig = SIG)
    {
        var file = new JObject { ["url"] = $"https://example.com/{name}.{ext}" };
        if (sha != null) file["sha256"] = sha;
        if (sig != null) file["sig256"] = sig;
        return new JObject
        {
            ["name"] = name,
            ["requires"] = new JObject { ["gte"] = gte },
            ["files"] = new JObject { [ext] = file }
        };
    }
}
