using System.Collections;
using NAPS2.App.Tests.Targets;

namespace NAPS2.App.Tests;

public class AppTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            yield return new object[] { new WindowsAppTestTarget() };
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return new object[] { new MacAppTestTarget() };
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return new object[] { new LinuxAppTestTarget() };
        }
#else
        yield return new object[] { new WindowsAppTestTarget() };
#endif
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}