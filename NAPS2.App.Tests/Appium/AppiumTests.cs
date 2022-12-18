using System.Threading;
using NAPS2.App.Tests.Targets;
using NAPS2.Sdk.Tests;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace NAPS2.App.Tests.Appium;

public class AppiumTests : ContextualTests
{
    protected WindowsDriver<WindowsElement> _session;

    private static WindowsDriver<WindowsElement> StartSession(AppTestExe exe, string appData)
    {
        var opts = new AppiumOptions();
        opts.AddAdditionalCapability("app", AppTestHelper.GetExePath(exe));
        opts.AddAdditionalCapability("appArguments", $"/Naps2TestData \"{appData}\"");
        return new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), opts);
    }

    public void Init(IAppTestTarget target)
    {
        _session = StartSession(target.Gui, FolderPath);
    }

    public override void Dispose()
    {
        _session.Dispose();
        base.Dispose();
    }

    protected void WaitUntilGone(string name, int timeoutInMs)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            while (true)
            {
                if (_session.FindElementsByName(name).Count == 0)
                {
                    break;
                }
                if (stopwatch.ElapsedMilliseconds > timeoutInMs)
                {
                    throw new WebDriverException("Timeout waiting for element to be gone");
                }
                Thread.Sleep(100);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    protected void ResetMainWindow()
    {
        _session.SwitchTo().Window(_session.WindowHandles[0]);
    }

    protected void ClickAt(WindowsElement element)
    {
#pragma warning disable CS0618
        // This is apparently obsolete, but the "correct" code is 10x as complicated so whatever
        _session.Mouse.Click(element.Coordinates);
#pragma warning restore CS0618
    }

    protected void ClickAtName(string name)
    {
        ClickAt(WaitAndFindElementByName(name));
    }

    protected void DoubleClickAt(WindowsElement element)
    {
#pragma warning disable CS0618
        // This is apparently obsolete, but the "correct" code is 10x as complicated so whatever
        _session.Mouse.MouseMove(element.Coordinates);
        _session.Mouse.DoubleClick(element.Coordinates);
#pragma warning restore CS0618
    }

    protected void DoubleClickAtName(string name)
    {
        DoubleClickAt(WaitAndFindElementByName(name));
    }

    protected WindowsElement WaitAndFindElementByName(string name)
    {
        int i = 0;
        while(true)
        {
            try
            {
                return _session.FindElementByName(name);
            }
            catch (WebDriverException)
            {
                if (++i > 10) throw;
                Thread.Sleep(100);
            }
        }
    }
}