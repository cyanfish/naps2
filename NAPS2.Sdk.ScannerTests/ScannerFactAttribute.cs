using Xunit;

namespace NAPS2.Sdk.ScannerTests;

public sealed class ScannerFactAttribute : FactAttribute
{
    public ScannerFactAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Scanner tests can only run when debugging as they require user interaction.";
        }
    }
}