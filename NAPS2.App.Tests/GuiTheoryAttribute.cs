using Xunit;

namespace NAPS2.App.Tests;

public class GuiTheoryAttribute : TheoryAttribute
{
    public GuiTheoryAttribute()
    {
        if (Environment.GetEnvironmentVariable("NAPS2_TEST_NOGUI") == "1")
        {
            Skip = "Running in headless mode, skipping GUI tests";
        }
    }
}