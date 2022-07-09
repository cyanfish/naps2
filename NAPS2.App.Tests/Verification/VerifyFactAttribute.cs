using Xunit;

namespace NAPS2.App.Tests.Verification;

// TODO: Consider a fact with multiple conditional attributes, e.g. verify-only, zip-only, requires-appium, win-only
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