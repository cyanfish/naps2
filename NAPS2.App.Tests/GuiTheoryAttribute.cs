using System.Runtime.CompilerServices;
using Xunit;

namespace NAPS2.App.Tests;

public class GuiTheoryAttribute : TheoryAttribute
{
    public GuiTheoryAttribute(
        [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("NAPS2_TEST_NOGUI") == "1")
        {
            Skip = "Running in headless mode, skipping GUI tests";
        }
    }
}