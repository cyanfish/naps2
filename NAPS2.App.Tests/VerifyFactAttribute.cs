using Xunit;

namespace NAPS2.App.Tests;

public sealed class VerifyFactAttribute : FactAttribute
{
    public VerifyFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NAPS2_TEST_ROOT")))
        {
            Skip = "This test only runs during verification ('n2 verify').";
        }
    }
}