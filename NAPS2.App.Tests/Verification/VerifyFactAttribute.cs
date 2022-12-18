using Xunit;

namespace NAPS2.App.Tests.Verification;

public sealed class VerifyTheoryAttribute : TheoryAttribute
{
    private bool _allowDebug;
    
    public VerifyTheoryAttribute()
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