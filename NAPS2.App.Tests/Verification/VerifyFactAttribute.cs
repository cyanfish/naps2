using Xunit;

namespace NAPS2.App.Tests.Verification;

// TODO: Consider a fact with multiple conditional attributes, e.g. verify-only, zip-only, requires-appium, win-only
public sealed class VerifyFactAttribute : FactAttribute
{
    private bool _allowDebug;
    
    public VerifyFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NAPS2_TEST_VERIFY")))
        {
            Skip = "This test only runs during verification ('n2 verify').";
        }
    }

    public bool AllowDebug
    {
        get => _allowDebug;
        set
        {
            if (value && Skip != null)
            {
                if (Debugger.IsAttached)
                {
                    Skip = null;
                }
                else
                {
                    Skip += " You can also debug this test to run it manually.";
                }
            }
            _allowDebug = value;
        }
    }
}