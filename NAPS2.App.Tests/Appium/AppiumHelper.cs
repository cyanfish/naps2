using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace NAPS2.App.Tests.Appium;

public static class AppiumHelper
{
    public static WindowsDriver<WindowsElement> StartSession(string exeName, string appData)
    {
        var opts = new AppiumOptions();
        opts.AddAdditionalCapability("app", AppTestHelper.GetExePath(exeName));
        opts.AddAdditionalCapability("appArguments", $"/Naps2TestData \"{appData}\"");
        return new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), opts);
    }
}