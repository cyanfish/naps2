using Xunit;

namespace NAPS2.App.Tests.Verification;

public sealed class VerifyTheoryAttribute : TheoryAttribute
{
    private bool _allowDebug;
    private bool _windowsAppium;
    
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

    public bool WindowsAppium
    {
        get => _windowsAppium;
        set
        {
#if NET6_0_OR_GREATER
            if (value && Skip == null && !OperatingSystem.IsWindows())
            {
                Skip = "Appium tests are only supported on Windows right now.";
            }
#endif
            _windowsAppium = value;
        }
    }
}