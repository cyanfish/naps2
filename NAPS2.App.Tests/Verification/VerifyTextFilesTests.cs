using Xunit;

namespace NAPS2.App.Tests.Verification;

public class VerifyTextFilesTests
{
    [VerifyTheory]
    [ClassData(typeof(InstallDirTestData))]
    public void License(string installDir)
    {
        var path = Path.Combine(installDir, "license.txt");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("This program is free software", text);
    }

    [VerifyTheory]
    [ClassData(typeof(InstallDirTestData))]
    public void Contributors(string installDir)
    {
        var path = Path.Combine(installDir, "contributors.txt");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("Primary NAPS2 developer", text);
    }

    [VerifyTheory]
    [ClassData(typeof(InstallDirTestData))]
    public void AppSettings(string installDir)
    {
        var path = Path.Combine(installDir, "appsettings.xml");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("<AppConfig>", text);
    }
}