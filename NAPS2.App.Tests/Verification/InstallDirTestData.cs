using System.Collections;

namespace NAPS2.App.Tests.Verification;

public class InstallDirTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NAPS2_TEST_VERIFY")))
        {
            yield return new object[] { Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT") };
        }
        else if (OperatingSystem.IsMacOS())
        {
            // No tests yet
        }
        else if (OperatingSystem.IsLinux())
        {
            // No tests yet
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}