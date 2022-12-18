using System.Collections;
using NAPS2.App.Tests.Targets;

namespace NAPS2.App.Tests.Appium;

public class AppiumTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return new object[] { new WinNet462AppTestTarget() };
        }
        else if (OperatingSystem.IsMacOS())
        {
            // No Appium impl yet
        }
        else if (OperatingSystem.IsLinux())
        {
            // No Appium impl yet
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}