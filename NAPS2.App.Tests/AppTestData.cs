using System.Collections;
using NAPS2.App.Tests.Targets;

namespace NAPS2.App.Tests;

public class AppTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return new object[] { new WinNet462AppTestTarget() };
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return new object[] { new MacAppTestTarget() };
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return new object[] { new LinuxAppTestTarget() };
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}