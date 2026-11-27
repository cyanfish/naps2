using System.Runtime.CompilerServices;
using Xunit;

namespace NAPS2.Sdk.ScannerTests;

public sealed class ScannerFactAttribute : FactAttribute
{
    public ScannerFactAttribute(
        [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Scanner tests can only run when debugging as they require user interaction.";
        }
    }
}