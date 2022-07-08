using Xunit;

namespace NAPS2.App.Tests;

public class VerifyTextFilesTests
{
    [VerifyFact]
    public void License()
    {
        var path = Path.Combine(AppTestHelper.GetBaseDirectory(), "license.txt");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("This program is free software", text);
    }

    [VerifyFact]
    public void Contributors()
    {
        var path = Path.Combine(AppTestHelper.GetBaseDirectory(), "contributors.txt");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("Primary NAPS2 developer", text);
    }
    
    [VerifyFact]
    public void AppSettings()
    {
        var path = Path.Combine(AppTestHelper.GetBaseDirectory(), "appsettings.xml");
        Assert.True(File.Exists(path));
        var text = File.ReadAllText(path);
        Assert.Contains("<AppConfig>", text);
    }
}