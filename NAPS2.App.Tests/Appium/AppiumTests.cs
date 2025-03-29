using System.Linq.Expressions;
using System.Threading;
using NAPS2.App.Tests.Targets;
using NAPS2.Sdk.Tests;
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
        Thread.Sleep(2000);
        _session = StartSession(target.Gui, FolderPath);
        ResetMainWindow();
    }

    public override void Dispose()
    {
        try
        {
            _session.Dispose();
        }
        catch (Exception)
        {
            // Ignore disposal errors
        }
        base.Dispose();
    }

    protected void ResetMainWindow()
    {
        _session.SwitchTo().Window(WaitFor(() => _session.WindowHandles.Single()));
    }

    protected T WaitFor<T>(Expression<Func<T>> expr, int timeoutInMs = 10_000)
    {
        var func = expr.Compile();
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                var value = func();
                if (value is null or false)
                {
                    throw new Exception();
                }
                return value;
            }
            catch (Exception)
            {
                if (stopwatch.ElapsedMilliseconds > timeoutInMs)
                {
                    throw new Exception($"Timed out waiting for \"{expr.Body}\"");
                }
                Thread.Sleep(100);
            }
        }
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
        ClickAt(WaitFor(() => _session.FindElementByName(name)));
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
        DoubleClickAt(WaitFor(() => _session.FindElementByName(name)));
    }

    protected bool HasElementWithName(string name)
    {
        return _session.FindElementsByName(name).Count > 0;
    }
}